using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
Console.WriteLine("=== Starting Web API with Authentication ===");
string? environment = builder.Environment.IsDevelopment() ? "Development" : "Production";
Console.WriteLine($"Environment: {environment}");



// 1. Configure Authentication (Cookie + OIDC)
if (builder.Environment.IsDevelopment())
{
    // Debug: verify ClientSecret is loaded from user secrets
    var clientSecret = builder.Configuration["AzureAd:ClientSecret"];
    Console.WriteLine($"=== DEV CONFIG DEBUG ===");
    Console.WriteLine($"ClientId: {builder.Configuration["AzureAd:ClientId"]}");
    Console.WriteLine($"ClientSecret present: {!string.IsNullOrEmpty(clientSecret)}");
    Console.WriteLine($"========================");

    builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

    // Add detailed OIDC logging AND explicitly set ClientSecret
    builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        // Explicitly set client secret for auth code redemption
        options.ClientSecret = clientSecret;

        options.Events = new OpenIdConnectEvents
        {
            OnRedirectToIdentityProvider = context =>
            {
                Console.WriteLine("=== OIDC REDIRECT ===");
                Console.WriteLine("Client Secret set: " + !string.IsNullOrEmpty(options.ClientSecret));
                Console.WriteLine($"Redirect URI: {context.ProtocolMessage.RedirectUri}");
                Console.WriteLine($"Request Host: {context.Request.Host}");
                Console.WriteLine($"Request Scheme: {context.Request.Scheme}");
                Console.WriteLine($"X-Forwarded-Host: {context.Request.Headers["X-Forwarded-Host"]}");
                Console.WriteLine($"X-Forwarded-Proto: {context.Request.Headers["X-Forwarded-Proto"]}");
                Console.WriteLine("=====================");
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("=== OIDC AUTH FAILED ===");
                Console.WriteLine($"Error: {context.Exception.Message}");
                Console.WriteLine($"Inner: {context.Exception.InnerException?.Message}");
                Console.WriteLine("========================");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("=== OIDC TOKEN VALIDATED ===");
                Console.WriteLine($"User: {context.Principal?.Identity?.Name}");
                Console.WriteLine("============================");
                return Task.CompletedTask;
            }
        };
    });
}
else
{
    // Production: Use Key Vault Certificate via Managed Identity
    Console.WriteLine("=== PROD: Setting up Key Vault Certificate Auth ===");

    // IMPORTANT: match your appsettings.json structure
    var keyVaultUrl = builder.Configuration["AzureAd:ClientCertificates:0:KeyVaultUrl"];
    var certName = builder.Configuration["AzureAd:ClientCertificates:0:KeyVaultCertificateName"];

    // This must be the *CLIENT ID* of the UAMI assigned to the Container App
    var managedIdentityClientId = "a8aa5450-479c-4437-87f9-891d2755d1b2";

    Console.WriteLine($"KeyVaultUrl: {keyVaultUrl}");
    Console.WriteLine($"CertName: {certName}");
    Console.WriteLine($"ManagedIdentityClientId: {managedIdentityClientId}");

    // Create credential for Key Vault access (your existing verification logging stays)
    var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
    {
        ManagedIdentityClientId = managedIdentityClientId,
        ExcludeVisualStudioCredential = true,
        ExcludeAzureCliCredential = true,
        ExcludeInteractiveBrowserCredential = true
    });

    // Verify certificate exists (keep your logging)
    try
    {
        var certClient = new CertificateClient(new Uri(keyVaultUrl!), credential);
        var cert = certClient.GetCertificate(certName);
        Console.WriteLine($"Certificate found: {cert.Value.Name}");
        Console.WriteLine($"Thumbprint: {BitConverter.ToString(cert.Value.Properties.X509Thumbprint).Replace("-", "")}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"=== CERTIFICATE VERIFICATION FAILED: {ex.Message} ===");
    }

    // Configure authentication (keep your structure)
    builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddMicrosoftIdentityWebApp(msIdentityOptions =>
        {
            builder.Configuration.GetSection("AzureAd").Bind(msIdentityOptions);

            // KEY FIX: Ensure MIW's KeyVaultCertificateLoader uses the correct UAMI
            msIdentityOptions.ClientCertificates = new[]
            {
                new CertificateDescription
                {
                    SourceType = CertificateSource.KeyVault,
                    KeyVaultUrl = keyVaultUrl,
                    KeyVaultCertificateName = certName,

                    // <-- THIS is the missing wiring in your current branch
                    ManagedIdentityClientId = managedIdentityClientId
                }
            };

            Console.WriteLine("=== Certificate configured for OIDC (KeyVault) ===");
        })
        .EnableTokenAcquisitionToCallDownstreamApi()
        .AddInMemoryTokenCaches();

  }  // Keep logging, but DO NOT overwrite Microsoft.Identity.Web's event handlers.



// 2. Configure Cookie
builder.Services.Configure<CookieAuthenticationOptions>(
    CookieAuthenticationDefaults.AuthenticationScheme,
    options =>
    {
        options.Cookie.Name = "AspireNuxtAuth";
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
    });
// Configure OIDC Cookies to fix "Nonce was null" error
builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    // Use authorization code flow - more reliable behind proxies
    options.ResponseType = "code";

    // Disable nonce validation - in BFF proxy pattern, the nonce cookie
    // is set on the internal domain but callback comes through public domain
    options.ProtocolValidator.RequireNonce = false;

    // Configure cookies for cross-site scenarios
    options.NonceCookie.SameSite = SameSiteMode.None;
    options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
    options.CorrelationCookie.SameSite = SameSiteMode.None;
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
    // Ensure cookies are visible to all paths
    options.NonceCookie.Path = "/";
    options.CorrelationCookie.Path = "/";
});
// 3. Configure Forwarded Headers (for Nuxt proxy)
// 3. Configure Forwarded Headers (for Nuxt proxy)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.All;
    options.ForwardLimit = null;  // No limit - trust all proxies
    options.KnownProxies.Clear();
    options.KnownIPNetworks.Clear();  // Use this instead
});

builder.Services.AddAuthorization();

var app = builder.Build();

var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All,
    ForwardLimit = null
};
forwardedHeadersOptions.KnownProxies.Clear();
forwardedHeadersOptions.KnownIPNetworks.Clear();

app.UseForwardedHeaders(forwardedHeadersOptions);

// Custom middleware to read our headers that bypass Envoy's rewriting
app.Use(async (context, next) =>
{
    // Read our custom headers that bypass Envoy's rewriting
    if (context.Request.Headers.TryGetValue("X-Public-Proto", out var proto) && !string.IsNullOrEmpty(proto))
    {
        context.Request.Scheme = proto.ToString();
        Console.WriteLine($"Set Scheme from X-Public-Proto: {proto}");
    }
    if (context.Request.Headers.TryGetValue("X-Public-Host", out var host) && !string.IsNullOrEmpty(host))
    {
        context.Request.Host = new HostString(host.ToString());
        Console.WriteLine($"Set Host from X-Public-Host: {host}");
    }

    await next();
});

// Then authentication, authorization, etc.
app.UseAuthentication();
app.UseAuthorization();



// Auth endpoints
app.MapGet("/auth/login", (string? returnUrl) =>
{
    Console.WriteLine($"=== LOGIN ENDPOINT HIT, returnUrl: {returnUrl} ===");
    return Results.Challenge(
        new AuthenticationProperties { RedirectUri = returnUrl ?? "/" },
        authenticationSchemes: [OpenIdConnectDefaults.AuthenticationScheme]);
});

app.MapGet("/auth/logout", () =>
{
    return Results.SignOut(
        new AuthenticationProperties { RedirectUri = "/" },
        authenticationSchemes: [OpenIdConnectDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme]);
});

app.MapGet("/auth/user", (HttpContext context) =>
{
    if (context.User.Identity?.IsAuthenticated != true)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(new
    {
        Name = context.User.Identity.Name,
        Claims = context.User.Claims.Select(c => new { c.Type, c.Value })
    });
}).RequireAuthorization();

// Weather endpoint
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/debug/headers", (HttpContext context) =>
{
    return Results.Ok(new
    {
        Host = context.Request.Host.ToString(),
        Scheme = context.Request.Scheme,
        PathBase = context.Request.PathBase.ToString(),
        XForwardedHost = context.Request.Headers["X-Forwarded-Host"].ToString(),
        XForwardedProto = context.Request.Headers["X-Forwarded-Proto"].ToString(),
        XForwardedFor = context.Request.Headers["X-Forwarded-For"].ToString()
    });
});
app.MapGet("/debug/raw-headers", (HttpContext context) =>
{
    var headers = context.Request.Headers
        .ToDictionary(h => h.Key, h => h.Value.ToString());

    return Results.Ok(new
    {
        Host = context.Request.Host.ToString(),
        Scheme = context.Request.Scheme,
        AllHeaders = headers
    });
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}