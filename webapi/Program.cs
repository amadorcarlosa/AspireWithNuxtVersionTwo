using System.IO.Pipes;
using System.Security.AccessControl;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Identity.Web;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore; // <--- ADDED: Required for Scalar UI

var builder = WebApplication.CreateBuilder(args);

// 1. SERVICES REGISTRATION
// ------------------------

// Required for Minimal APIs to be discovered by OpenAPI
builder.Services.AddEndpointsApiExplorer(); 

// The new .NET 9/10 OpenAPI generator
builder.Services.AddOpenApi();

Console.WriteLine("=== Starting Web API with Authentication ===");
string? environment = builder.Environment.IsDevelopment() ? "Development" : "Production";
Console.WriteLine($"Environment: {environment}");


// --- AUTHENTICATION CONFIGURATION (Your existing code) ---
if (builder.Environment.IsDevelopment())
{
    var clientSecret = builder.Configuration["AzureAd:ClientSecret"];
    Console.WriteLine($"=== DEV CONFIG DEBUG ===");
    Console.WriteLine($"ClientId: {builder.Configuration["AzureAd:ClientId"]}");
    Console.WriteLine($"ClientSecret present: {!string.IsNullOrEmpty(clientSecret)}");
    Console.WriteLine($"========================");

    builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

    builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.ClientSecret = clientSecret;
        options.Events = new OpenIdConnectEvents
        {
            OnRedirectToIdentityProvider = context =>
            {
                Console.WriteLine($"=== OIDC REDIRECT URI: {context.ProtocolMessage.RedirectUri} ===");
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"=== OIDC AUTH FAILED: {context.Exception.Message} ===");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine($"=== OIDC TOKEN VALIDATED: {context.Principal?.Identity?.Name} ===");
                return Task.CompletedTask;
            }
        };
    });
}
else
{
    Console.WriteLine("=== PROD: Setting up Key Vault Certificate Auth ===");
    var keyVaultUrl = builder.Configuration["AzureAd:ClientCertificates:0:KeyVaultUrl"];
    var certName = builder.Configuration["AzureAd:ClientCertificates:0:KeyVaultCertificateName"];
    var managedIdentityClientId = builder.Configuration["Azure:ManagedIdentityClientId"]; // Load from config if possible

    var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
    {
        ManagedIdentityClientId = managedIdentityClientId,
        ExcludeVisualStudioCredential = true,
        ExcludeAzureCliCredential = true,
        ExcludeInteractiveBrowserCredential = true
    });

    builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddMicrosoftIdentityWebApp(msIdentityOptions =>
        {
            builder.Configuration.GetSection("AzureAd").Bind(msIdentityOptions);
            msIdentityOptions.ClientCertificates = new[]
            {
                new CertificateDescription
                {
                    SourceType = CertificateSource.KeyVault,
                    KeyVaultUrl = keyVaultUrl,
                    KeyVaultCertificateName = certName,
                    ManagedIdentityClientId = managedIdentityClientId
                }
            };
        })
        .EnableTokenAcquisitionToCallDownstreamApi()
        .AddInMemoryTokenCaches();
}

builder.Services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.Name = "AspireNuxtAuth";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.ResponseType = "code";
    options.ProtocolValidator.RequireNonce = false;
    options.NonceCookie.SameSite = SameSiteMode.None;
    options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
    options.CorrelationCookie.SameSite = SameSiteMode.None;
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddAuthorization();


var app = builder.Build();

// 2. MIDDLEWARE PIPELINE
// ----------------------

// ---> SCALAR & OPENAPI SETUP (This was missing) <---
if (app.Environment.IsDevelopment())
{
    // Generates the /openapi/v1.json file
    app.MapOpenApi();

    // Serves the Scalar UI at /scalar/v1
    app.MapScalarApiReference();
}

// Proxy Headers Logic
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All,
    ForwardLimit = null
};
forwardedHeadersOptions.KnownProxies.Clear();
forwardedHeadersOptions.KnownIPNetworks.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

app.Use(async (context, next) =>
{
    if (context.Request.Headers.TryGetValue("X-Public-Proto", out var proto))
        context.Request.Scheme = proto.ToString();
    
    if (context.Request.Headers.TryGetValue("X-Public-Host", out var host))
        context.Request.Host = new HostString(host.ToString());

    await next();
});

app.UseAuthentication();
app.UseAuthorization();


// 3. ENDPOINTS
// ------------

app.MapGet("/auth/login", (string? returnUrl) =>
{
    return Results.Challenge(
        new AuthenticationProperties { RedirectUri = returnUrl ?? "/" },
        authenticationSchemes: [OpenIdConnectDefaults.AuthenticationScheme]);
})
.WithName("Login")   // <--- Name needed for Scalar
.WithTags("Auth");   // <--- Tags group it in Scalar

app.MapGet("/auth/logout", () =>
{
    return Results.SignOut(
        new AuthenticationProperties { RedirectUri = "/" },
        authenticationSchemes: [OpenIdConnectDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme]);
})
.WithName("Logout")
.WithTags("Auth");

app.MapGet("/auth/user", (HttpContext context) =>
{
    if (context.User.Identity?.IsAuthenticated != true) return Results.Unauthorized();
    return Results.Ok(new { Name = context.User.Identity.Name });
})
.WithName("GetUser")
.WithTags("Auth");

app.MapGet("/speech/token", async (IConfiguration config, HttpClient client) =>
{
    // 1. READ CONFIG
    var region = config["Azure:Speech:Region"];
    var managedIdentityClientId = config["Azure:ManagedIdentityClientId"];
    var tenantId = config["Azure:TenantId"]; 

    // --- DEBUG LOGGING ---
    Console.WriteLine("--------------------------------------------------");
    Console.WriteLine($"[Config Check] Region:    '{region}'");
    Console.WriteLine($"[Config Check] ClientId:  '{managedIdentityClientId}'");
    Console.WriteLine($"[Config Check] TenantId:  '{tenantId}'");
    Console.WriteLine("--------------------------------------------------");

    // Safety check
    if (string.IsNullOrEmpty(region) || string.IsNullOrEmpty(managedIdentityClientId) || string.IsNullOrEmpty(tenantId))
    {
        Console.WriteLine("[Error] One or more configuration values are MISSING.");
        return Results.Problem("Missing Config: Check Region, ClientId, and TenantId.");
    }

    try 
    {
        // 2. AUTHENTICATE
        Console.WriteLine($"[Auth] Attempting login to Tenant: {tenantId}...");
        
        var credentialOptions = new DefaultAzureCredentialOptions
        {
            ManagedIdentityClientId = managedIdentityClientId,
            TenantId = tenantId
        };
        var credential = new DefaultAzureCredential(credentialOptions);
        
        // 3. GET AZURE AD TOKEN
        Console.WriteLine("[Auth] Requesting Azure AD Token...");
        var aadToken = await credential.GetTokenAsync(
            new TokenRequestContext(new[] { "https://cognitiveservices.azure.com/.default" })
        );
        Console.WriteLine("[Auth] Azure AD Token acquired successfully.");

        // 4. EXCHANGE FOR SPEECH TOKEN
        var subdomain = "vue-mathtabla-voice"; 
        var fetchUri = $"https://{subdomain}.cognitiveservices.azure.com/sts/v1.0/issueToken";
        
        Console.WriteLine($"[Exchange] Posting to: {fetchUri}");

        using var request = new HttpRequestMessage(HttpMethod.Post, fetchUri);
        request.Headers.Add("Authorization", $"Bearer {aadToken.Token}");
        
        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[Azure Failure] Status: {response.StatusCode}, Reason: {error}");
            return Results.Problem($"Azure refused the token: {error}");
        }

        var speechToken = await response.Content.ReadAsStringAsync();
        Console.WriteLine("[Success] Speech Token retrieved!");
        
        return Results.Ok(new { token = speechToken, region });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Exception] Critical Crash: {ex.Message}");
        return Results.Problem($"Server Crash: {ex.Message}");
    }
})
.WithName("GetSpeechToken")
.WithTags("Speech")
.RequireAuthorization();

// Weather Forecast
var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
app.MapGet("/weatherforecast", (HttpContext context) =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return Results.Ok(forecast);
})
.WithName("GetWeatherForecast")
.WithTags("Weather")
.RequireAuthorization();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}