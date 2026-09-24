using Azure.Monitor.OpenTelemetry.AspNetCore;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Models;
using KosmosERP.Models.Interfaces;
using KosmosERP.Api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        public static ConfigurationSettings
                        ConfigureSettings(this IServiceCollection services, ConfigurationManager configManager)
        {
            var authenticationSettings = new AuthenticationSettings()
            {
                APIPrivateKey = GetSettingValue(configManager, "APIPrivateKey", "AuthenticationSettings"),
                AuthenticationProvider = GetSettingValue(configManager, "AuthenticationProvider", "AuthenticationSettings"),
                Authority = GetSettingValue(configManager, "Authority", "AuthenticationSettings"),
                Audience = GetSettingValue(configManager, "Audience", "AuthenticationSettings"),
                TokenURL = GetSettingValue(configManager, "TokenURL", "AuthenticationSettings"),
                ClientSecret = GetSettingValue(configManager, "ClientSecret", "AuthenticationSettings"),
                BaseURL = GetSettingValue(configManager, "BaseURL", "AuthenticationSettings"),
                Realm = GetSettingValue(configManager, "Realm", "AuthenticationSettings"),
                IdPCertificate = GetSettingValue(configManager, "IdPCertificate", "AuthenticationSettings"),
                SingleLogoutURL = GetSettingValue(configManager, "SingleLogoutURL", "AuthenticationSettings"),
            };


            var storageAccountSettings = new FileStorageSettings()
            {
                account_provider = GetSettingValue(configManager, "FileStorageAccountProvider", "FileStorageSettings"),
                azure_connection_string = GetSettingValue(configManager, "AzureStorageConnectionString", "FileStorageSettings"),
                azure_container_name = GetSettingValue(configManager, "AzureContainerName", "FileStorageSettings"),
                azure_access_key = GetSettingValue(configManager, "AzureAccessKey", "FileStorageSettings"),
                aws_access_key = GetSettingValue(configManager, "AWSAccessKey", "FileStorageSettings"),
                aws_secret_key = GetSettingValue(configManager, "AWSSecretKey", "FileStorageSettings"),
                aws_bucket_name = GetSettingValue(configManager, "AWSBucketName", "FileStorageSettings"),
                aws_region = GetSettingValue(configManager, "AWSRegion", "FileStorageSettings"),
                gpc_json_file_path = GetSettingValue(configManager, "GPCJsonFilePath", "FileStorageSettings"),
                gpc_bucket_name = GetSettingValue(configManager, "GPCBucketName", "FileStorageSettings"),
                local_storage_path = GetSettingValue(configManager, "LocalStoragePath", "FileStorageSettings")
            };


            var messagePublisherSettings = new MessagePublisherSettings()
            {
                account_provider = GetSettingValue(configManager, "MessagePublisherAccountProvider", "MessagePublisherSettings"),
                rabbitmq_host = GetSettingValue(configManager, "RabbitMQHost", "MessagePublisherSettings"),
                rabbitmq_username = GetSettingValue(configManager, "RabbitMQUsername", "MessagePublisherSettings"),
                rabbitmq_password = GetSettingValue(configManager, "RabbitMQPassword", "MessagePublisherSettings"),
                rabbitmq_port = GetSettingValue(configManager, "RabbitMQPort", "MessagePublisherSettings"),
                rabbitmq_virtual_host = GetSettingValue(configManager, "RabbitMQVirtualHost", "MessagePublisherSettings"),
                rabbitmq_exchange = GetSettingValue(configManager, "RabbitMQExchange", "MessagePublisherSettings"),
                aws_region = GetSettingValue(configManager, "AWSRegion", "MessagePublisherSettings"),
                azure_connection_string = GetSettingValue(configManager, "AzureBusConnectionString", "MessagePublisherSettings"),
                rabbitmq_routing_key = GetSettingValue(configManager, "RabbitMQRoutingKey", "MessagePublisherSettings"),
                transaction_movement_topic = GetSettingValue(configManager, "TransactionMovementTopic", "MessagePublisherSettings")
            };

            var paymentProviderSettings = new PaymentProviderSettings()
            {
                payment_provider = GetSettingValue(configManager, "PaymentProvider", "PaymentProviderSettings"),
                square_token = GetSettingValue(configManager, "SquareToken", "PaymentProviderSettings"),
                stripe_api_key = GetSettingValue(configManager, "StripeApiKey", "PaymentProviderSettings")
            };

            var logProviderSettings = new LogProviderSettings()
            {
                log_provider = GetSettingValue(configManager, "LogProvider", "LogProviderSettings"),
                application_insights_connection_string = GetSettingValue(configManager, "ApplicationInsightsConnectionString", "LogProviderSettings"),
                datadog_api_key = GetSettingValue(configManager, "DatadogApiKey", "LogProviderSettings"),
                datadog_endpoint = GetSettingValue(configManager, "DatadogSite", "LogProviderSettings"),
                datadog_environment = GetSettingValue(configManager, "DatadogEnv", "LogProviderSettings"),
                database_connection_string = GetSettingValue(configManager, "DatabaseConnectionString", "LogProviderSettings")
            };

            var hangFireSettings = new HangfireSettings()
            {
                HangfireConnectionString = GetSettingValue(configManager, "HangfireConnectionString", "HangfireSettings")
            };

            var databaseSettings = new DatabaseSettings()
            {
                DatabaseConnectionString = GetSettingValue(configManager, "DatabaseConnectionString", "DatabaseSettings")
            };

            var shippingSetings = new ShippingSettings()
            {
                shipping_provider = GetSettingValue(configManager, "ShippingProvider", "ShippingSettings"),
                ship_station_api_key = GetSettingValue(configManager, "ShipStationApiKey", "ShippingSettings"),
            };

            services.AddSingleton<IAuthenticationSettings>(authenticationSettings);
            services.AddSingleton<IFileStorageSettings>(storageAccountSettings);
            services.AddSingleton<IMessagePublisherSettings>(messagePublisherSettings);
            services.AddSingleton<IPaymentProviderSettings>(paymentProviderSettings);
            services.AddSingleton<ILogProviderSettings>(logProviderSettings);
            services.AddSingleton<IHangfireSettings>(hangFireSettings);
            services.AddSingleton<IDatabaseSettings>(databaseSettings);
            services.AddSingleton<IShippingSettings>(shippingSetings);

            return new ConfigurationSettings(
                authenticationSettings,
                storageAccountSettings,
                messagePublisherSettings,
                paymentProviderSettings,
                logProviderSettings,
                hangFireSettings,
                shippingSetings,
                databaseSettings
            );
        }

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
                        IssuerSigningKey = TokenModule.CreateSecurityKey(authenticationSettings.APIPrivateKey)
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
        public static void AddDataDogLogging(this IServiceCollection services, ILogProviderSettings logProviderSettings)
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
                .Enrich.WithProperty("env", logProviderSettings.datadog_environment ?? "dev")
                .WriteTo.DatadogLogs(
                    apiKey: logProviderSettings.datadog_api_key,
                    source: "csharp",
                    service: "Kosmos-erp-api",
                    host: Environment.MachineName,
                    configuration: datadogConfig)
                .CreateLogger();
        }

        public static void AddOpenTelemetryLogging(this IServiceCollection services, ILogProviderSettings logProviderSettings)
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

        private static string? GetSettingValue(ConfigurationManager configManager, string settingName, string configRoot)
        {
            if(configManager.GetSection("Settings").Exists())
            {
                return configManager.GetSection("Settings").GetSection(configRoot).GetValue<string>(settingName);
            }
            else
            {
                return Environment.GetEnvironmentVariable(settingName);
            }
        }
    }
}
