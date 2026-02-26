using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("econocomBPC");

var adminPassword = builder.AddParameter("admin-password", secret: true);

var seq = builder.AddSeq("seq", 5341)
                 .WithDataVolume()
                 .WithExternalHttpEndpoints()
                 .WithLifetime(ContainerLifetime.Persistent)
                 .PublishAsDockerComposeService((r, s) => { s.Name = "seq"; });

var mailpit = builder.AddMailPit("mailpit")
                .WithLifetime(ContainerLifetime.Persistent)
                .WithExternalHttpEndpoints()
                .WithDataVolume("maildata")
                .PublishAsDockerComposeService((r, s) => { s.Name = "mailpit"; });

var db = builder.AddSqlServer("sqlserver", adminPassword)
    .WithLifetime(ContainerLifetime.Persistent)
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
    .PublishAsDockerComposeService((dcsr, serv) => { serv.Name = nameof(WebApp); });


await builder.Build().RunAsync();
