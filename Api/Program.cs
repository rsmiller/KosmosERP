using Hangfire;
using Hangfire.MySql;
using KosmosERP.Api;
using KosmosERP.Api.Filters;
using KosmosERP.Api.Middleware;
using KosmosERP.BusinessLayer;
using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Models;
using KosmosERP.Models.Interfaces;
using KosmosERP.Module;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Serilog;



var builder = WebApplication.CreateBuilder(args);


/// For development debug
///

Environment.SetEnvironmentVariable("DatabaseConnectionString", "server=192.168.1.148;uid=auser;pwd=12345;database=kosmos_erp_new");
Environment.SetEnvironmentVariable("HangfireConnectionString", "server=192.168.1.148;uid=auser;pwd=12345;database=hangfire;Allow User Variables=true");
Environment.SetEnvironmentVariable("MessagePublisherAccountProvider", "Database");

Environment.SetEnvironmentVariable("TransactionMovementTopic", "transaction_movement");


Environment.SetEnvironmentVariable("FileStorageAccountProvider", "local");
Environment.SetEnvironmentVariable("APIUsername", "test");
Environment.SetEnvironmentVariable("APIPassword", "password!234");
Environment.SetEnvironmentVariable("APIPrivateKey", "3a8&8bef2*8f5DD!22237hyA$&$hag2@UyHjs");
Environment.SetEnvironmentVariable("LocalStoragePath", "/home/ryan/Downloads/uploads");
Environment.SetEnvironmentVariable("PaymentProvider", "Stripe");
Environment.SetEnvironmentVariable("StripeApiKey", "sk_test_");
Environment.SetEnvironmentVariable("LogProvider", "database");

Environment.SetEnvironmentVariable("AuthenticationProvider", "database");
Environment.SetEnvironmentVariable("Realm", "");
Environment.SetEnvironmentVariable("BaseURL", "");
Environment.SetEnvironmentVariable("Authority", "");
Environment.SetEnvironmentVariable("Audience", "");
Environment.SetEnvironmentVariable("TokenURL", "");
Environment.SetEnvironmentVariable("IdPCertificate", "");
Environment.SetEnvironmentVariable("ClientSecret", "");
Environment.SetEnvironmentVariable("SingleLogoutURL", "");

//Environment.SetEnvironmentVariable("DD_API_KEY", "");
//Environment.SetEnvironmentVariable("DD_SITE", "");
//Environment.SetEnvironmentVariable("DD_ENV", "dev");
//Environment.SetEnvironmentVariable("DD_LOGS_INJECTION", "true");
//Environment.SetEnvironmentVariable("DD_LOGS_DIRECT_SUBMISSION_INTEGRATIONS", "Serilog");
//Environment.SetEnvironmentVariable("LogProvider", "DataDog");

//Environment.SetEnvironmentVariable("AzureStorageConnectionString", "");
//Environment.SetEnvironmentVariable("FileStorageAccountProvider", "azure");
//Environment.SetEnvironmentVariable("AzureContainerName", "documents");

//Environment.SetEnvironmentVariable("FileStorageAccountProvider", "aws");
//Environment.SetEnvironmentVariable("AWSRegion", "us-east-1");
////Environment.SetEnvironmentVariable("AWSBucketName", "");
//Environment.SetEnvironmentVariable("AWSAccessKey", "");
//Environment.SetEnvironmentVariable("AWSSecretKey", "");

//Environment.SetEnvironmentVariable("MessagePublisherAccountProvider", "Azure");
//Environment.SetEnvironmentVariable("AzureBusConnectionString", "");
//Environment.SetEnvironmentVariable("LogProvider", "Azure");
//Environment.SetEnvironmentVariable("ApplicationInsightsConnectionString", "");


// Add services to the container.

// Add detailed problem details for better error responses
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
        ctx.ProblemDetails.Extensions["instance"] = $"{ctx.HttpContext.Request.Method} {ctx.HttpContext.Request.Path}";
    };
});

builder.Services.AddControllers(options =>
{
    // Add filters to log model validation and controller activation errors
    options.Filters.Add<ValidationErrorLoggingFilter>();
    options.Filters.Add<ControllerActivationLoggingFilter>();
})
.ConfigureApiBehaviorOptions(options =>
{
    // Customize 400 Bad Request responses
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value.Errors.Count > 0)
            .Select(e => new
            {
                Field = e.Key,
                Errors = e.Value.Errors.Select(x => x.ErrorMessage).ToArray()
            })
            .ToList();

        var problemDetails = new ValidationProblemDetails(context.ModelState)
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "One or more validation errors occurred.",
            Status = 400,
            Detail = "See the errors property for details.",
            Instance = context.HttpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        
        // Log the validation errors
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(
            "Model validation failed for {Method} {Path}. Errors: {Errors}",
            context.HttpContext.Request.Method,
            context.HttpContext.Request.Path,
            System.Text.Json.JsonSerializer.Serialize(errors)
        );

        return new BadRequestObjectResult(problemDetails);
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        In = ParameterLocation.Header,
        Name = "Authorization", // Standard header name
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter your JWT token"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});

builder.Services.AddSignalR();
builder.Services.AddMemoryCache();


//////////////////////////////////////////////////////////////////////////////////////////////////////////
//
var authenticationSettings = new AuthenticationSettings()
{
    APIPrivateKey = Environment.GetEnvironmentVariable("APIPrivateKey"),
    AuthenticationProvider = Environment.GetEnvironmentVariable("AuthenticationProvider"),
    Authority = Environment.GetEnvironmentVariable("Authority"),
    Audience = Environment.GetEnvironmentVariable("Audience"),
    TokenURL = Environment.GetEnvironmentVariable("TokenURL"),
    ClientSecret = Environment.GetEnvironmentVariable("ClientSecret"),
    BaseURL = Environment.GetEnvironmentVariable("BaseURL"),
    Realm = Environment.GetEnvironmentVariable("Realm"),
    IdPCertificate = Environment.GetEnvironmentVariable("IdPCertificate"),
    SingleLogoutURL = Environment.GetEnvironmentVariable("SingleLogoutURL"),
};

builder.Services.AddSingleton<IAuthenticationSettings>(authenticationSettings);


var storageAccountSettings = new FileStorageSettings()
{
    account_provider = Environment.GetEnvironmentVariable("FileStorageAccountProvider"),
    azure_connection_string = Environment.GetEnvironmentVariable("AzureStorageConnectionString"),
    azure_container_name = Environment.GetEnvironmentVariable("AzureContainerName"),
    azure_access_key = Environment.GetEnvironmentVariable("AzureAccessKey"),
    aws_access_key = Environment.GetEnvironmentVariable("AWSAccessKey"),
    aws_secret_key = Environment.GetEnvironmentVariable("AWSSecretKey"),
    aws_bucket_name = Environment.GetEnvironmentVariable("AWSBucketName"),
    aws_region = Environment.GetEnvironmentVariable("AWSRegion"),
    gpc_json_file_path = Environment.GetEnvironmentVariable("GPCJsonFilePath"),
    gpc_bucket_name = Environment.GetEnvironmentVariable("GPCBucketName"),
    local_storage_path = Environment.GetEnvironmentVariable("LocalStoragePath")
};

builder.Services.AddSingleton<IFileStorageSettings>(storageAccountSettings);


///////////////////////////////////////////////////////////////////////////////////////////////////
// Factories
var sorage_provider = StorageFactory.Create(storageAccountSettings);
builder.Services.AddSingleton<IStorageProvider>(sorage_provider);


var messagePublisherSettings = new MessagePublisherSettings()
{
    account_provider = Environment.GetEnvironmentVariable("MessagePublisherAccountProvider"),
    rabbitmq_host = Environment.GetEnvironmentVariable("RabbitMQHost"),
    rabbitmq_username = Environment.GetEnvironmentVariable("RabbitMQUsername"),
    rabbitmq_password = Environment.GetEnvironmentVariable("RabbitMQPassword"),
    rabbitmq_port = Environment.GetEnvironmentVariable("RabbitMQPort"),
    rabbitmq_virtual_host = Environment.GetEnvironmentVariable("RabbitMQVirtualHost"),
    rabbitmq_exchange = Environment.GetEnvironmentVariable("RabbitMQExchange"),
    aws_region = Environment.GetEnvironmentVariable("AWSRegion"),
    azure_connection_string = Environment.GetEnvironmentVariable("AzureBusConnectionString"),
    rabbitmq_routing_key = Environment.GetEnvironmentVariable("RabbitMQRoutingKey"),
    transaction_movement_topic = Environment.GetEnvironmentVariable("TransactionMovementTopic")
};


builder.Services.AddSingleton<IMessagePublisherSettings>(messagePublisherSettings);
builder.Services.AddScoped<IMessageFactory, MessageFactory>();

/////////////////////////////////////////////////////////////////////////////////////////////////////
// Payment Provider

var paymentProviderSettings = new PaymentProviderSettings()
{
    payment_provider = Environment.GetEnvironmentVariable("PaymentProvider"),
    square_token = Environment.GetEnvironmentVariable("SquareToken"),
    stripe_api_key = Environment.GetEnvironmentVariable("StripeApiKey")
};

builder.Services.AddSingleton<IPaymentProviderSettings>(paymentProviderSettings);
builder.Services.AddScoped<IPaymentProviderFactory, PaymentProviderFactory>();

if(paymentProviderSettings.payment_provider.ToLower() == PaymentProviderType.Stripe)
{
    Stripe.StripeConfiguration.ApiKey = paymentProviderSettings.stripe_api_key;
}

//////////////////////////////////////////////////////////////////////////////////////////////////////
/// Log Provider
var logProviderSettings = new LogProviderSettings()
{
    log_provider = Environment.GetEnvironmentVariable("LogProvider"),
    application_insights_connection_string = Environment.GetEnvironmentVariable("ApplicationInsightsConnectionString"),
    datadog_api_key = Environment.GetEnvironmentVariable("DD_API_KEY"),
    datadog_endpoint = Environment.GetEnvironmentVariable("DD_SITE"),
    database_connection_string = Environment.GetEnvironmentVariable("DatabaseConnectionString")
};

builder.Services.AddSingleton<ILogProviderSettings>(logProviderSettings);
builder.Services.AddScoped<ILogProviderFactory, LogProviderFactory>();


if(logProviderSettings.log_provider.ToLower() == LogProviderType.Azure)
{
    builder.Services.AddOpenTelemetryLogging(logProviderSettings);

    builder.Logging.ClearProviders();
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
    builder.Logging.AddSerilog(Log.Logger, dispose: true);
}
else if(logProviderSettings.log_provider.ToLower() == LogProviderType.DataDog)
{
    builder.Services.AddDataDogLogging(logProviderSettings);

    builder.Logging.ClearProviders();
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
    builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
    builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
    builder.Logging.AddSerilog();
}

///////////////////////////////////////////////////////////////////////////////////////////////////
/// LOGS
/// 


///////////////////////////////////////////////////////////////////////////////////////////////////
// Hangfire
//
builder.Services.AddHangfire(config => config
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseStorage(
        new MySqlStorage(Environment.GetEnvironmentVariable("HangfireConnectionString"), new MySqlStorageOptions
        {
            TablesPrefix = "Hangfire_", // optional prefix for tables
            TransactionIsolationLevel = (System.Transactions.IsolationLevel?)System.Data.IsolationLevel.ReadCommitted,
            QueuePollInterval = TimeSpan.FromSeconds(15),
            JobExpirationCheckInterval = TimeSpan.FromHours(1),
            CountersAggregateInterval = TimeSpan.FromMinutes(5),
            PrepareSchemaIfNecessary = true,
        }))
    );
builder.Services.AddHangfireServer();

// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

builder.Services.AddModules();

builder.Services.AddDbContext<IBaseERPContext, ERPDbContext>(options => options.UseMySQL(Environment.GetEnvironmentVariable("DatabaseConnectionString")));

//////////////////////////////////////////////////////////////////////////////////////////////////////////
// Memory Service
builder.Services.AddMemoryServices();

//////////////////////////////////////////////////////////////////////////////////////////////////////////
// Add authentication

if (authenticationSettings.AuthenticationProvider.ToLower() == AuthenticiationProviders.Keycloak)
{
    builder.Services.AddKeycloakAuthentication(authenticationSettings);
    //builder.Services.AddAuthorization();
}
else
{
    builder.Services.AddJwtAuthentication(authenticationSettings);
}

builder.Services.AddScoped<IAuthenticationFactory, AuthenticationFactory>();

// Custom authorization seam for [ERPAuthorize]. Defers to the default claim/role
// check until ErpCustomAuthorizationHandler is implemented.
builder.Services.AddScoped<KosmosERP.Api.Authorization.IERPAuthorizationHandler, KosmosERP.Api.Authorization.ErpCustomAuthorizationHandler>();


var app = builder.Build();

//////////////////////////////////////////////////////////////////////////////////////////////////////////
/// Run all module initialization methods
/// 
var scope = app.Services.CreateScope();

var modules = typeof(UserModule).Assembly.GetTypes()
    .Where(x => x.IsClass && x.GetInterface(nameof(IBaseERPModule)) == typeof(IBaseERPModule)).ToList();


foreach (var module in modules)
{
    var class_interface = module.GetInterfaces().Where(m => m.Namespace.Contains("KosmosERP.BusinessLayer.Modules")).FirstOrDefault();

    if (class_interface != null)
    {
        var activated_module = scope.ServiceProvider.GetService(class_interface);

        if (activated_module != null)
        {
            var casted_module = (IBaseERPModule)activated_module;
            try
            {
                casted_module.StartUp();
                casted_module.SeedPermissions();
            }
            catch (Exception) { }
        }
    }
}


var activated_mem_cache = scope.ServiceProvider.GetService(typeof(IMemoryCacheService<KeyValueStore>));

if (activated_mem_cache != null)
{
    var casted_cache= (IMemoryCacheService<KeyValueStore>)activated_mem_cache;
    try
    {
        casted_cache.Setup();
    }
    catch (Exception) { }
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////
// Configure the HTTP request pipeline.

// Add global exception handler
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.MapOpenApi();
app.MapSwagger().RequireAuthorization();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "KosmosERP API V1");

});



app.UseHangfireDashboard("/hangfire");

app.UseHttpsRedirection();

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


// Hubs
app.MapHub<NotificationModule>("/notification_hub");



//////////////////////////////////////////////////////////////////////////////////////////////////////////
// Hangfire apps
//
RecurringJob.AddOrUpdate<TransactionJob>("transaction-job", job => job.Run(), Cron.Minutely);
RecurringJob.AddOrUpdate<BillingPaymentEntryJob>("billing-payment-entry-job", job => job.Run(), Cron.Minutely);
RecurringJob.AddOrUpdate<InventoryCalculatorJob>("inventory-calculator-job", job => job.Run(), Cron.MinuteInterval(10));
RecurringJob.AddOrUpdate<PaymentsJob>("payment-job", job => job.Run(), Cron.MinuteInterval(2));

app.Run();
