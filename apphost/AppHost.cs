using Aspire.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// Add Container App Environment for Azure deployment
builder.AddAzureContainerAppEnvironment("env");

var webapi = builder.AddProject<Projects.webapi>("server")
    .WithExternalHttpEndpoints();

builder.Build().Run();
