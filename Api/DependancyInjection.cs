using Azure.Monitor.OpenTelemetry.AspNetCore;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Models;
using KosmosERP.Models.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
using System.Security.Claims;
using System.Text.Json;

namespace KosmosERP.Api
{
    public static class DependancyInjection
    {
        public static void AddJwtAuthentication(this IServiceCollection services, IAuthenticationSettings authenticationSettings)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(m =>
            {
                m.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateIssuerSigningKey = true,
                        ValidateAudience = false,
                        ValidIssuer = TokenModule.Issuer,
                        IssuerSigningKey = TokenModule.CreateSecurityKey(Environment.GetEnvironmentVariable("APIPrivateKey"))
                    };
            });
        }
        public static void AddKeycloakAuthentication(this IServiceCollection services, IAuthenticationSettings authenticationSettings)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authenticationSettings.Authority;
                options.RequireHttpsMetadata = false; // set false only for dev
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5), // Allow 5 min clock skew
                    NameClaimType = "preferred_username",
                    RoleClaimType = ClaimTypes.Role
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var identity = context.Principal!.Identity as ClaimsIdentity;
                        if (identity == null) return Task.CompletedTask;

                        // -------- Realm Roles --------
                        var realmAccess = context.Principal.FindFirst("realm_access")?.Value;
                        if (realmAccess != null)
                        {
                            using var doc = JsonDocument.Parse(realmAccess);
                            if (doc.RootElement.TryGetProperty("roles", out var roles))
                            {
                                foreach (var role in roles.EnumerateArray())
                                {
                                    var roleValue = role.GetString()!;
                                    if (!identity.HasClaim(ClaimTypes.Role, roleValue))
                                        identity.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                                }
                            }
                        }

                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine("=== AUTH FAILED ===");
                        Console.WriteLine($"Exception Type: {context.Exception.GetType().Name}");
                        Console.WriteLine($"Message: {context.Exception.Message}");
                        if (context.Exception.InnerException != null)
                        {
                            Console.WriteLine($"Inner Exception: {context.Exception.InnerException.Message}");
                        }
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        Console.WriteLine("=== AUTH CHALLENGE ===");
                        Console.WriteLine($"Error: {context.Error}");
                        Console.WriteLine($"Error Description: {context.ErrorDescription}");
                        return Task.CompletedTask;
                    }
                };
            });
        }
        public static void AddDataDogLogging(this IServiceCollection services, LogProviderSettings logProviderSettings)
        {
            var datadogConfig = new Serilog.Sinks.Datadog.Logs.DatadogConfiguration
            {
                Url = $"https://http-intake.logs.{logProviderSettings.datadog_endpoint}"
            };

            services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .AddHttpClientInstrumentation()
                    .AddSource("KosmosERP")
                    .AddOtlpExporter(opts =>
                    {
                        opts.Endpoint = new Uri($"https://otlp.{logProviderSettings.datadog_endpoint}/v1/traces");

                        // Required for Datadog
                        opts.Headers = $"DD-API-KEY={logProviderSettings.datadog_api_key}";
                    });
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter("Kosmos.Metrics")
                    .AddOtlpExporter(opts =>
                    {
                        opts.Endpoint = new Uri($"https://otlp.{logProviderSettings.datadog_endpoint}/v1/metrics");
                        opts.Headers = $"DD-API-KEY={logProviderSettings.datadog_api_key}";
                    });
            });


            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("System.Net.Http.HttpClient", Serilog.Events.LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("env", Environment.GetEnvironmentVariable("DD_ENV") ?? "dev")
                .WriteTo.DatadogLogs(
                    apiKey: logProviderSettings.datadog_api_key,
                    source: "csharp",
                    service: "Kosmos-erp-api",
                    host: Environment.MachineName,
                    configuration: datadogConfig)
                .CreateLogger();
        }

        public static void AddOpenTelemetryLogging(this IServiceCollection services, LogProviderSettings logProviderSettings)
        {
            services.AddOpenTelemetry()
            .UseAzureMonitor(options =>
            {
                options.ConnectionString = logProviderSettings.application_insights_connection_string;
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddHttpClientInstrumentation()
                    .AddSource("KomosERP");
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter("KosmosERP.Metrics");
            });

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning)
                .WriteTo.ApplicationInsights(
                    connectionString: logProviderSettings.application_insights_connection_string,
                    telemetryConverter: new TraceTelemetryConverter())
                .Enrich.FromLogContext()
                .CreateLogger();

        }
    }
}
