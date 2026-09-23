using Hangfire;
using Hangfire.MySql;
using KosmosERP.Api;
using KosmosERP.Api.Authorization;
using KosmosERP.Api.Filters;
using KosmosERP.Api.Middleware;
using KosmosERP.Api.Models;
using KosmosERP.BusinessLayer;
using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Models;
using KosmosERP.Models.Interfaces;
using KosmosERP.Module;
using KosmosERP.Reporting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Serilog;

/////////////////////////////////////////////////////////////////////////..////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////
var builder = WebApplication.CreateBuilder(args);


//////////////////////////////////////////////////////////////////////////////////////////////////////////
// Configure settings from environment variables or from appsettings.json

var settingsConfigurations = builder.Services.ConfigureSettings(builder.Configuration);
var authenticationSettings = settingsConfigurations.Item1;
var storageAccountSettings = settingsConfigurations.Item2;
var messagePublisherSettings = settingsConfigurations.Item3;
var paymentProviderSettings = settingsConfigurations.Item4;
var logProviderSettings = settingsConfigurations.Item5;
var hangfireSettings = settingsConfigurations.Item6;
var databaseSettings = settingsConfigurations.Item7;


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


///////////////////////////////////////////////////////////////////////////////////////////////////
// Factories
var sorage_provider = StorageFactory.Create(storageAccountSettings);
builder.Services.AddSingleton<IStorageProvider>(sorage_provider);


builder.Services.AddScoped<IMessageFactory, MessageFactory>();
builder.Services.AddScoped<IPaymentProviderFactory, PaymentProviderFactory>();

/////////////////////////////////////////////////////////////////////////////////////////////////////
// Payment Provider


if (paymentProviderSettings.payment_provider.ToLower() == PaymentProviderType.Stripe)
{
    Stripe.StripeConfiguration.ApiKey = paymentProviderSettings.stripe_api_key;
}

//////////////////////////////////////////////////////////////////////////////////////////////////////
/// Log Provider

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
// Hangfire
//
builder.Services.AddHangfire(config => config
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseStorage(
        new MySqlStorage(hangfireSettings.HangfireConnectionString, new MySqlStorageOptions
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

// Reporting module (FastReport-based reports) — service, module identity, and generators.
builder.Services.AddReporting();

// General reports catalog shown on the Reports page, loaded once from reportscatalog.json.
var reportCatalogJson = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "reportscatalog.json"));
builder.Services.AddSingleton<IReportCatalog>(new ReportCatalog(reportCatalogJson));

builder.Services.AddDbContext<IBaseERPContext, ERPDbContext>(options => options.UseMySQL(databaseSettings.DatabaseConnectionString));

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
builder.Services.AddScoped<IERPAuthorizationHandler, ErpCustomAuthorizationHandler>();


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


// The Reporting module lives outside the BusinessLayer assembly scanned above, so
// register its module row and permissions explicitly here (idempotent).
var reporting_module = scope.ServiceProvider.GetService<IReportingModule>();
if (reporting_module != null)
{
    try
    {
        reporting_module.StartUp();
        reporting_module.SeedPermissions();
    }
    catch (Exception) { }
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
