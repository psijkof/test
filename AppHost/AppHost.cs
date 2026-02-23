using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("compose")
    .WithDashboard(true)
    .ConfigureComposeFile(composeFile =>
    {
        composeFile.Services["dashboard"]
            .Ports.AddRange(["18888:18888"]);
    });

var adminPassword = builder.AddParameter("admin-password", secret: true);

var db = builder.AddSqlServer("sqlserver", adminPassword)
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsDockerComposeService((r, s) => { s.Name = "sqlserver"; })
    .AddDatabase("EcoBpc");

builder.AddProject<WebApp>(nameof(WebApp), "https")
    .WithExternalHttpEndpoints()
    .WithReference(db)
    .WaitFor(db)
    .PublishAsDockerComposeService((dcsr, serv) => { serv.Name = nameof(WebApp); });


await builder.Build().RunAsync();
