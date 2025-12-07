using Projects;
using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);


// Add the following line to configure the Azure App Container environment
builder.AddAzureContainerAppEnvironment("env");



builder.Build().Run();
var webapi=builder.AddProject<webapi>("server")
.WithHttpEndpoint(port: 5000, name: "api-internal");
string? port=Environment.GetEnvironmentVariable("PORT");
if(port!=null){
Console.WriteLine($"PORT={port}");
}
else
{
Console.WriteLine("PORT not set, defaulting to 3000");
}

// 3) Add Scalar API Reference (unified UI in Aspire dashboard)
var scalar = builder.AddScalarApiReference(options =>
{
    // Global theming/customization for the dashboard UI
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



builder.Build().Run();
