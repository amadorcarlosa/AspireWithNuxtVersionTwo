
using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureContainerAppEnvironment("env");

var webapi = builder.AddProject<Projects.webapi>("server");

if (builder.ExecutionContext.IsPublishMode)
{
    var frontend = builder.AddNodeApp("frontend", "../webapp", "start")
        .WithPnpm()
        .WithHttpEndpoint(env: "PORT")
        .WithExternalHttpEndpoints()
        .WithReference(webapi)
        .WaitFor(webapi)
        .WithEnvironment("ApiUrl", webapi.GetEndpoint("https"));
}
else
{
    var scalar = builder.AddScalarApiReference(options =>
    {
        options.WithTheme(ScalarTheme.Purple);
    });
    scalar.WithApiReference(webapi);
    
    var frontend = builder.AddJavaScriptApp("frontend", "../webapp")
        .WithPnpm()
        .WithRunScript("dev")
        .WithHttpEndpoint(port: 3000, targetPort: 3000, isProxied: false) // Fixed port
        .WithExternalHttpEndpoints()
        .WithReference(webapi)
        .WaitFor(webapi)
        .WithEnvironment("ApiUrl", webapi.GetEndpoint("https"));
}

builder.Build().Run();
