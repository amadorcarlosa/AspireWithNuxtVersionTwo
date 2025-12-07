using Aspire.Hosting;
using Projects;
using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);


// Add the following line to configure the Azure App Container environment
 builder.AddAzureContainerAppEnvironment("env");



var webapi=builder.AddProject<Projects.webapi>("server");


// 3) Add Scalar API Reference (unified UI in Aspire dashboard)


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
else{
    var scalar = builder.AddScalarApiReference(options =>
{
    // Global theming/customization for the dashboard
    options.WithTheme(ScalarTheme.Purple);
    // You can also set other global options here (hide sidebar, etc.)
});
scalar.WithApiReference(webapi);
var frontend = builder.AddJavaScriptApp("frontend", "../webapp")
    .WithPnpm()
    .WithRunScript("dev")
    .WithHttpEndpoint(env: "PORT") // <--- This must be the string "PORT"
    .WithExternalHttpEndpoints()
    .WithReference(webapi)
    .WaitFor(webapi)
    .WithEnvironment("ApiUrl", webapi.GetEndpoint("https"));
}



builder.Build().Run();
