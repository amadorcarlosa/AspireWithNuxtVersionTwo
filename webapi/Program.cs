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
    Console.WriteLine("=== PROD: Setting up Key Vault Certificate Auth ===");

    // You can keep these prints if you want (they're harmless),
    // but DO NOT manually read ClientCredentials:* if your config uses ClientCertificates.
    var managedIdentityClientId = "a8aa5450-479c-4437-87f9-891d2755d1b2";

    // ✅ Let Microsoft.Identity.Web bind *your existing* appsettings.json:
    // AzureAd:ClientCertificates + SendX5C, etc.
    builder.Services
        .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
        .EnableTokenAcquisitionToCallDownstreamApi()
        .AddInMemoryTokenCaches();

    // ✅ If you're using a USER-assigned managed identity, MIW needs to know which one.
    // (Alternative is setting env var AZURE_CLIENT_ID to this value.)
    builder.Services.Configure<MicrosoftIdentityOptions>(options =>
    {
        options.UserAssignedManagedIdentityClientId = managedIdentityClientId;
    });

    // ✅ Keep your logging, but DO NOT replace Events. Chain them.
    builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.Events ??= new OpenIdConnectEvents();

        var priorRedirect = options.Events.OnRedirectToIdentityProvider;
        options.Events.OnRedirectToIdentityProvider = async context =>
        {
            Console.WriteLine("=== PROD OIDC REDIRECT ===");
            context.ProtocolMessage.RedirectUri = "https://amadorcarlos.com/api/signin-oidc";
            Console.WriteLine($"RedirectUri: {context.ProtocolMessage.RedirectUri}");
            Console.WriteLine("==========================");

            if (priorRedirect is not null) await priorRedirect(context);
        };

        var priorFailed = options.Events.OnAuthenticationFailed;
        options.Events.OnAuthenticationFailed = async context =>
        {
            Console.WriteLine("=== PROD OIDC AUTH FAILED ===");
            Console.WriteLine($"Error: {context.Exception.Message}");
            Console.WriteLine($"Inner: {context.Exception.InnerException?.Message}");
            Console.WriteLine("=============================");

            if (priorFailed is not null) await priorFailed(context);
        };

        var priorValidated = options.Events.OnTokenValidated;
        options.Events.OnTokenValidated = async context =>
        {
            Console.WriteLine("=== PROD TOKEN VALIDATED ===");
            Console.WriteLine($"User: {context.Principal?.Identity?.Name}");
            Console.WriteLine("============================");

            if (priorValidated is not null) await priorValidated(context);
        };
    });
}
 


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