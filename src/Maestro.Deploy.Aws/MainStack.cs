using System;
using System.Collections.Generic;
using System.IO;
using Constructs;
using HashiCorp.Cdktf;
using HashiCorp.Cdktf.Providers.Aws.IamRole;
using HashiCorp.Cdktf.Providers.Aws.IamRolePolicy;
using HashiCorp.Cdktf.Providers.Aws.LambdaEventSourceMapping;
using HashiCorp.Cdktf.Providers.Aws.LambdaFunction;
using HashiCorp.Cdktf.Providers.Aws.Provider;
using HashiCorp.Cdktf.Providers.Aws.S3Bucket;
using HashiCorp.Cdktf.Providers.Aws.S3Object;
using HashiCorp.Cdktf.Providers.Aws.SqsQueue;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;


namespace Maestro.MyApp
{
    class MainStack : TerraformStack
    {
        public MainStack(Construct scope, string id) : base(scope, id)
        {
            new S3Backend(this,
                new S3BackendConfig
                {
                    Bucket = "tfstate.omny.ca", Key = "maestro-dotnet.tfstate", Region = "us-east-1"
                });

            // define resources here
            new AwsProvider(this, "AWS", new AwsProviderConfig { Region = "us-east-1" });

            S3Bucket bucket = new S3Bucket(this, "maestro-lambda-deploys",
                new S3BucketConfig { Bucket = "maestro-lambda-deploys" });

            // find publish dir

            // get directory of the application
            var start = AppContext.BaseDirectory;
            var parent = new System.IO.DirectoryInfo(start);
            string zipFileName = "Maestro.Web.zip";

            // check if file exists
            while (parent != null && parent.Exists && parent != parent.Root &&
                   !File.Exists(Path.Combine(parent.FullName, zipFileName)))
            {
                parent = parent.Parent;
            }

            string zipFilePath = Path.Combine(parent!.FullName, zipFileName);
            if (!File.Exists(zipFilePath))
            {
                throw new Exception("Could not find publish zip");
            }

            // convert zipFilePath to absolute
            zipFilePath = Path.GetFullPath(zipFilePath);

            var eventZipFilePath = Path.Combine(parent!.FullName, "Maestro.Events.Runner.zip");
            if (!File.Exists(eventZipFilePath))
            {
                throw new Exception("Could not find events zip");
            }


            S3Object webArchive = new S3Object(this, "maestro-lambda-deploy-web",
                new S3ObjectConfig
                {
                    Bucket = bucket.Bucket,
                    Key = $"maestro-web-deploy-{DateTime.Now.Ticks}.zip",
                    Source = zipFilePath
                });

            S3Object eventsArchive = new S3Object(this, "maestro-lambda-deploy-events",
                new S3ObjectConfig
                {
                    Bucket = bucket.Bucket,
                    Key = $"maestro-events-deploy-{DateTime.Now.Ticks}.zip",
                    Source = eventZipFilePath
                });

            var lambdaExecutionRole = new IamRole(this, "maestro-web-role", new IamRoleConfig
            {
                AssumeRolePolicy = """
                                   {
                                       "Version": "2012-10-17",
                                       "Statement": [
                                           {
                                               "Effect": "Allow",
                                               "Principal": {
                                                   "Service": [
                                                       "lambda.amazonaws.com"
                                                   ]
                                               },
                                               "Action": "sts:AssumeRole"
                                           }
                                       ]
                                   }
                                   """,
                InlinePolicy = new[]
                {
                    new IamRoleInlinePolicy
                    {
                        Policy = """
                                 {
                                     "Version": "2012-10-17",
                                     "Statement": [
                                         {
                                             "Effect": "Allow",
                                             "Action": "*",
                                             "Resource": "*"
                                         }
                                     ]
                                 }
                                 """,
                        Name = "maestro-web-role-policy"
                    }
                },
                Name = "maestro-web-role",
                Description = "Role for Maestro Web Lambda",
            });

            var dlq = new SqsQueue(this, "dlq",
                new SqsQueueConfig { Name = "maestro-event-dlq.fifo", FifoQueue = true });

            var eventQueue = new SqsQueue(this, "event-queue", new SqsQueueConfig
            {
                Name = "maestro-event-queue.fifo",
                FifoQueue = true,
                RedrivePolicy = @"{
                    ""deadLetterTargetArn"": """ + dlq.Arn + @""",
                    ""maxReceiveCount"": 3
                }"
            });

            Dictionary<string, string> keyVariables = new Dictionary<string, string>
            {
                { "Events__SqsQueueUrl", eventQueue.Url },
                { "JWT_SECRET", Environment.GetEnvironmentVariable("JWT_SECRET") },
                { "CONNECTION_STRING", Environment.GetEnvironmentVariable("CONNECTION_STRING") },
                { "Metadata__TmdbKey", Environment.GetEnvironmentVariable("Metadata__TmdbKey") }
            };
            Dictionary<string, string> sensitiveVariables = new();
            foreach (var key in keyVariables.Keys)
            {
                var sensitive = new TerraformVariable(this, $"maestro-sensitive-value-{key.ToLower()}",
                    new TerraformVariableConfig { Type = "string", Default = keyVariables[key], Sensitive = true });
                sensitiveVariables.Add(key, sensitive.StringValue);
            }

            //var sensitiveVariables = keyVariables;//(Dictionary<string,string>)sensitiveValues.Value;


            var lambdaFunction = new LambdaFunction(this, "maestro-web-dotnet",
                new LambdaFunctionConfig
                {
                    Runtime = "dotnet8",
                    S3Bucket = webArchive.Bucket,
                    Role = lambdaExecutionRole.Arn,
                    S3Key = webArchive.Key,
                    FunctionName = "maestro-web-dotnet-lambda",
                    Handler = "Maestro.Web",
                    MemorySize = 512,
                    Timeout = 30,
                    Environment = new LambdaFunctionEnvironment { Variables = sensitiveVariables }
                });


            var eventHandler = new LambdaFunction(this, "maestro-events-dotnet-lambda",
                new LambdaFunctionConfig
                {
                    Runtime = "dotnet8",
                    S3Bucket = eventsArchive.Bucket,
                    S3Key = eventsArchive.Key,
                    Handler = "Maestro.Events.Runner",
                    Role = lambdaExecutionRole.Arn,
                    FunctionName = "maestro-event-dotnet-lambda",
                    MemorySize = 512,
                    Timeout = 30,
                    Environment = new LambdaFunctionEnvironment { Variables = sensitiveVariables }
                });

            new LambdaEventSourceMapping(this, "maestro-events-dotnet",
                new LambdaEventSourceMappingConfig
                {
                    EventSourceArn = eventQueue.Arn, FunctionName = eventHandler.FunctionName
                });

            var bucketName = Environment.GetEnvironmentVariable("UI_BUCKET")!;
            //var uiBucket = new CloudfrontBucket(this, bucketName, "maestro-web-");
        }
    }
}
