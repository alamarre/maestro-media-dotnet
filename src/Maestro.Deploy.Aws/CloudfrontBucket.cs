// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using HashiCorp.Cdktf;
using HashiCorp.Cdktf.Providers.Aws.CloudfrontDistribution;
using HashiCorp.Cdktf.Providers.Aws.S3Bucket;

namespace Maestro.MyApp;

public class CloudfrontBucket
{
    public S3Bucket Bucket { get; }
    public CloudfrontDistribution Distribution { get; }
    
    public CloudfrontBucket(
        TerraformStack stack, 
        string bucketName,
        string baseName, 
        string? acmCert = null, 
        string[]? aliases = null
    ) {
        Bucket = new S3Bucket(stack, baseName,
            new S3BucketConfig
            {
                Bucket = bucketName,
                Website = new S3BucketWebsite
                    {
                    IndexDocument = "index.html",
                    ErrorDocument = "index.html"
                }
            });
        var distributionConfig = new CloudfrontDistributionConfig
        {
            Enabled = true,
            Aliases = aliases,
            DefaultCacheBehavior = new CloudfrontDistributionDefaultCacheBehavior
            {
                AllowedMethods = new[] { "GET", "HEAD" },
                TargetOriginId = "myS3Origin",
                ViewerProtocolPolicy = "redirect-to-https",
                ForwardedValues = new CloudfrontDistributionDefaultCacheBehaviorForwardedValues
                {
                    QueryString = false,
                    Cookies = new CloudfrontDistributionDefaultCacheBehaviorForwardedValuesCookies
                    {
                        Forward = "none"
                    }
                }
            },
            Origin = new[]
            {
                new CloudfrontDistributionOrigin
                {
                    DomainName = Bucket.BucketDomainName,
                    OriginId = "myS3Origin",
                    CustomOriginConfig = new CloudfrontDistributionOriginCustomOriginConfig
                    {
                        HttpPort = 80,
                        HttpsPort = 443,
                        OriginProtocolPolicy = "http-only",
                        OriginSslProtocols = new[] { "TLSv1.2" }
                    }
                }
            }
        };

        if (acmCert != null)
        {
            distributionConfig.ViewerCertificate = new CloudfrontDistributionViewerCertificate
            {
                AcmCertificateArn = acmCert, 
                SslSupportMethod = "sni-only"
            };
        }

        Distribution = new CloudfrontDistribution(stack, $"{baseName}-cloudfront", distributionConfig);
    }
}
