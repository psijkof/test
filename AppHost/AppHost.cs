using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("econocomBPC")
    .WithDashboard(enabled: true)
    .WithSshDeploySupport(); 

var adminPassword = builder.AddParameter("admin-password", secret: true);

var seq = builder.AddSeq("seq", 5341)
    .WithExternalHttpEndpoints()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithExternalHttpEndpoints()
    .WithDataVolume("seqdata")
    .PublishAsDockerComposeService((r, s) => { s.Name = "seq"; });

var mailpit = builder.AddMailPit("mailpit")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithExternalHttpEndpoints()
    .WithDataVolume("maildata")
    .PublishAsDockerComposeService((r, s) => { s.Name = "mailpit"; });

var db = builder.AddSqlServer("sqlserver", adminPassword)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("sqldata")
    .PublishAsDockerComposeService((r, s) => { s.Name = "sqlserver"; })
    .AddDatabase("EcoBpc");

builder.AddProject<WebApp>(nameof(WebApp), "https")
    .WithExternalHttpEndpoints()
    .WithReference(db)
    .WaitFor(db)
    .WithReference(seq)
    .WaitFor(seq)
    .WithReference(mailpit)
    .WaitFor(mailpit)
    .PublishAsDockerComposeService((dcsr, serv) => { serv.Name = nameof(WebApp); serv.Hostname = ""; });

await builder.Build().RunAsync();
