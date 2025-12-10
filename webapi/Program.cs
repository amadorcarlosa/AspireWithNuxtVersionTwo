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
    builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
    
    // Add detailed OIDC logging
    builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.Events = new OpenIdConnectEvents
        {
            OnRedirectToIdentityProvider = context =>
            {
                Console.WriteLine("=== OIDC REDIRECT ===");
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
    builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(options =>
        {
            builder.Configuration.GetSection("AzureAd").Bind(options);

            options.Events = new OpenIdConnectEvents
            {
                OnRedirectToIdentityProvider = context =>
                {
                    Console.WriteLine("=== PROD OIDC REDIRECT ===");
                    Console.WriteLine($"Original RedirectUri: {context.ProtocolMessage.RedirectUri}");
                    Console.WriteLine($"Request Host: {context.Request.Host}");
                    Console.WriteLine($"Request Scheme: {context.Request.Scheme}");
                    Console.WriteLine($"X-Forwarded-Host: {context.Request.Headers["X-Forwarded-Host"]}");
                    Console.WriteLine($"X-Forwarded-Proto: {context.Request.Headers["X-Forwarded-Proto"]}");
                    Console.WriteLine($"X-Forwarded-For: {context.Request.Headers["X-Forwarded-For"]}");

                    // Force the public redirect
                    var forced = "https://amadorcarlos.com/api/signin-oidc";
                    context.ProtocolMessage.RedirectUri = forced;

                    Console.WriteLine($"Forced RedirectUri: {forced}");
                    Console.WriteLine("==========================");

                    return Task.CompletedTask;
                },

                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine("=== PROD OIDC AUTH FAILED ===");
                    Console.WriteLine($"Error: {context.Exception.Message}");
                    Console.WriteLine($"Inner: {context.Exception.InnerException?.Message}");
                    Console.WriteLine("=============================");
                    return Task.CompletedTask;
                },

                OnTokenValidated = context =>
                {
                    Console.WriteLine("=== PROD TOKEN VALIDATED ===");
                    Console.WriteLine($"User: {context.Principal?.Identity?.Name}");
                    Console.WriteLine("============================");
                    return Task.CompletedTask;
                }
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

// 3. Configure Forwarded Headers (for Nuxt proxy)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | 
                               ForwardedHeaders.XForwardedProto | 
                               ForwardedHeaders.XForwardedHost;
    options.ForwardLimit = 1;
    options.KnownProxies.Clear();
    options.KnownIPNetworks.Clear();
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Add request logging middleware
app.Use(async (context, next) =>
{
    Console.WriteLine($"=== REQUEST: {context.Request.Method} {context.Request.Path} ===");
    Console.WriteLine($"Host: {context.Request.Host}");
    Console.WriteLine($"X-Forwarded-Host: {context.Request.Headers["X-Forwarded-Host"]}");
    Console.WriteLine($"X-Forwarded-Proto: {context.Request.Headers["X-Forwarded-Proto"]}");
    Console.WriteLine($"X-Forwarded-For: {context.Request.Headers["X-Forwarded-For"]}");
    await next();
});

// Add this BEFORE app.UseAuthentication()
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All,
    RequireHeaderSymmetry = false
};
// Trust all proxies in container environment
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardedHeadersOptions);

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

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}