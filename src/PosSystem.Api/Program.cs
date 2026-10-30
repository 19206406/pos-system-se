using Identity.Module;
using Microsoft.AspNetCore.HttpOverrides;
using PosSystem.Api.ExceptionHandling;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddSingleton(TimeProvider.System);

// Behind a reverse proxy the real client IP (sessions, rate limiting) comes from X-Forwarded-For.
// Only loopback proxies are trusted by default; add the proxy or ingress addresses in ForwardedHeaders:KnownProxies.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    foreach (var proxy in builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? [])
        options.KnownProxies.Add(System.Net.IPAddress.Parse(proxy));
});

/// <summary> shared exception </summary>
builder.Services.AddGlobalExceptionHandling();

/// <summary> modules </summary>
builder.Services.AddIdentityModule(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
// Turns empty 401, 403, 404 and 429 responses (authentication, authorization, rate limiting) into ProblemDetails
app.UseStatusCodePages();
app.UseForwardedHeaders();
app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    /// <summary> scalar configuration </summary>
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference("/api-docs", options =>
    {
        options.WithTitle("Pos-system API Documentation");
    }).AllowAnonymous();
}

app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllers();

app.Run();
