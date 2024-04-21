using Aspire.Hosting;
using Projects;

//using Projects;

var builder = DistributedApplication.CreateBuilder(args);
var db = builder.AddPostgres("pg")
    .WithDataVolume()
    .AddDatabase("postgresdb");

var api = builder.AddProject<Projects.Maestro_Web>("api")
    .WithReference(db);
var blazor = builder.AddProject<Projects.Maestro_Web_Ui>("blazor");
builder.Build().Run();
