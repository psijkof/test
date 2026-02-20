using Json.More;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var test = builder.AddProject<Projects.WebApp>(nameof(Projects.WebApp)).Resource.ToJsonDocument();

builder.Build().Run();
