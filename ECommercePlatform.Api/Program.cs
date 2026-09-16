using ECommercePlatform.Api.Configuration;
using ECommercePlatform.Api.Extensions;
using ECommercePlatform.Application;
using ECommercePlatform.Application.BulkImport;
using ECommercePlatform.Infrastructure;
using ECommercePlatform.Infrastructure.Services;
using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using Serilog;

// A bootstrap logger captures failures that happen before the real Serilog
// pipeline is configured — otherwise a bad connection string or missing signing
// key produces a silent crash.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddApplicationLogging();

    // Each layer registers what it owns. Program.cs stays a table of contents
    // rather than a list of every service in the system.
    builder.Services
        .AddApiServices(builder.Configuration)
        .AddApplication(builder.Configuration)
        .AddInfrastructure(builder.Configuration, builder.Environment);

builder.Services.AddScoped<IBulkImportService, BulkImportService>();

    builder.Services
        .AddJwtAuthentication()
        .AddApplicationCors(builder.Configuration)
        .AddApplicationRateLimiting()
        .AddApiBehavior()
        .AddApplicationOpenApi();

    var app = builder.Build();

    // ---- Pre-flight check ----
    // A second API instance (e.g. F5 in Visual Studio while the background API
    // from start-backend.cmd is already listening) would otherwise die deep in
    // Kestrel with a confusing IOException and exit code 1. Detect it up front
    // and say what to do.
    foreach (var url in (Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? string.Empty)
        .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Port > 0 && PortIsInUse(uri.Host, uri.Port))
        {
            Log.Fatal(
                "Port {Port} is already in use by another ECommercePlatform API instance (including the background one started by start-backend.cmd). " +
                "Stop it first with stop-backend.cmd, then run this instance again - or use start-backend.cmd / _run-api.cmd instead of F5.",
                uri.Port);
            return 3;
        }
    }

    // ---- Request pipeline. Order matters. ----

    // Outermost, so nothing escapes as an unformatted 500.
    app.UseExceptionHandler();

    // Establishes the correlation id that every log line and problem response quotes.
    app.UseRequestId();

    app.UseApplicationRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        // Serves the OpenAPI document itself at /openapi/v1.json.
        app.MapOpenApi();

        // Swagger UI at /swagger, pointed at that same document. This is the
        // UI-only Swashbuckle package: document generation stays with
        // Microsoft.AspNetCore.OpenApi, so there is one document, not two.
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "ECommercePlatform API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "ECommercePlatform API";

            // Keeps the bearer token across page reloads, so you are not
            // re-pasting it after every change.
            options.EnablePersistAuthorization();
        });

        // Scalar remains available at /scalar/v1 for anyone who prefers it.
        app.MapScalarApiReference();
    }
    else
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseCors(CorsOptions.PolicyName);

    // Ahead of authentication so unauthenticated floods are shed cheaply.
    app.UseRateLimiter();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Liveness answers "is the process up"; readiness also checks the database.
    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = _ => false
    }).AllowAnonymous();

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    }).AllowAnonymous();

    using (var scope = app.Services.CreateScope())
    {
        await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().MigrateAsync();
    }

    Log.Information("ECommercePlatform API starting in {Environment}.", app.Environment.EnvironmentName);

    await app.RunAsync();

    return 0;
}
// The EF Core design-time tools build the host and then abort it on purpose;
// that is not a start-up failure.
catch (HostAbortedException)
{
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "ECommercePlatform API terminated unexpectedly during start-up.");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

static bool PortIsInUse(string host, int port)
{
    var address = (host, IPAddress.TryParse(host, out var parsed)) switch
    {
        ("localhost", _) or ("*", _) or ("", _) or (_, false) => IPAddress.Any,
        _ => parsed ?? IPAddress.Any
    };
    try
    {
        var listener = new TcpListener(address, port);
        listener.Start();
        listener.Stop();
        return false;
    }
    catch (SocketException)
    {
        return true;
    }
}
