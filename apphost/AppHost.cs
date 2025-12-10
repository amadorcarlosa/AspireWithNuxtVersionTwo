using Scalar.Aspire;

#pragma warning disable ASPIREACADOMAINS001

var builder = DistributedApplication.CreateBuilder(args);

// Your specific domain and certificate
var customDomain = builder.AddParameter("customDomain", "amadorcarlos.com", publishValueAsDefault: true);
var certificateName = builder.AddParameter("certificateName", "amadorcarlos.com-envebis4-251209215047", publishValueAsDefault: true);

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
        .WithEnvironment("ApiUrl", webapi.GetEndpoint("https"))
        .PublishAsAzureContainerApp((module, app) =>
        {
            app.ConfigureCustomDomain(customDomain, certificateName);
        });
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
        .WithHttpEndpoint(port: 3000, targetPort: 3000, isProxied: false)
        .WithExternalHttpEndpoints()
        .WithReference(webapi)
        .WaitFor(webapi)
        .WithEnvironment("ApiUrl", webapi.GetEndpoint("https"));
}

builder.Build().Run();