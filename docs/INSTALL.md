# Installation

## Introduction
Installation can be done a variety of ways depending on your needs:

 - Compiled source
 - Docker images

## Configuration
If you are using a compiled source, the default configuration is done through the [appsettings.json](http://github.com/rsmiller/KosmosERP/blob/main/Api/appsettings.json) file, although you can use environment variables if you want or need to. Simply delete the appsettings.json or don't deploy it in the CI/CD pipeline.

All configuration environment variables and associated values will match that in the appsettings.json. These variables are:

**AuthenticationSettings**

-   `APIPrivateKey`
-   `AuthenticationProvider`
-   `Authority`
-   `Audience`
-   `TokenURL`
-   `ClientSecret`
-   `BaseURL`
-   `Realm`
-   `IdPCertificate`
-   `SingleLogoutURL`

**FileStorageSettings**

-   `FileStorageAccountProvider`
-   `AzureStorageConnectionString`
-   `AzureContainerName`
-   `AzureAccessKey`
-   `AWSAccessKey`
-   `AWSSecretKey`
-   `AWSBucketName`
-   `AWSRegion`
-   `GPCJsonFilePath`
-   `GPCBucketName`
-   `LocalStoragePath`

**MessagePublisherSettings**

-   `MessagePublisherAccountProvider`
-   `RabbitMQHost`
-   `RabbitMQUsername`
-   `RabbitMQPassword`
-   `RabbitMQPort`
-   `RabbitMQVirtualHost`
-   `RabbitMQExchange`
-   `AWSRegion` (also in FileStorageSettings)
-   `AzureBusConnectionString`
-   `RabbitMQRoutingKey`
-   `TransactionMovementTopic`

**PaymentProviderSettings**

-   `PaymentProvider`
-   `SquareToken`
-   `StripeApiKey`

**LogProviderSettings**

-   `LogProvider`
-   `ApplicationInsightsConnectionString`
-   `DatadogApiKey`
-   `DatadogSite`
-   `DatadogEnv`
-   `DatabaseConnectionString` (also in DatabaseSettings)

**HangfireSettings**

-   `HangfireConnectionString`

**DatabaseSettings**

-   `DatabaseConnectionString`

**ShippingSettings**

-   `ShippingProvider`
-   `ShipStationApiKey`

The providers variables are associated with these possible values:
| Env variable | Enum class | Possible values |
|---|---|---|
| `AuthenticationProvider` | `AuthenticiationProviders` | `database`, `keycloak`, `saml`, `mock` |
| `FileStorageAccountProvider` | `StorageType` | `local`, `azure`, `aws`, `google`, `mock` |
| `MessagePublisherAccountProvider` | `MessagePublisherType` | `database`, `rabbit`, `azure`, `aws`, `google`, `mock` |
| `PaymentProvider` | `PaymentProviderType` | `square`, `stripe`, `paypal`, `mock` |
| `LogProvider` | `LogProviderType` | `azure`, `datadog`, `database`, `mock` |
| `ShippingProvider` | `ShippingProviderType` | `database`, `ship_station`, `mock` |

## Compiled Source
### API
Like any other .NET project, simply build the project. The /bin/ output is what you'll put in the IIS folder. You can also configure Ngnix or Apache to read and execute

### UI
The command `npm run build` compiles the UI into static pages. This output can be dropped into any web server. However, there are configuration elements for the UI as well. You will need to utilize environment variables or a .env.local file that will store the variables. Those variables with example values are:

    NEXT_PUBLIC_API_BASE_URL="https://localhost:7054"
    NEXT_PUBLIC_AUTHORIZATION_REALM="your-realm-here"
    NEXT_PUBLIC_AUTHORIZATION_CLIENT_ID="your-client-id-here"
    NEXT_PUBLIC_AUTH_METHOD="database"
    NEXT_PUBLIC_SAML_LOGIN_URL="https://localhost:7054/saml/login"
    NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY="your-stripe-publishable-key-here"

## Docker
You can grab the Docker images from this project's packages section or host your own. You can spin these up in Kubernetes, AKS, or Rancher depending on your needs. The API project's [Dockerfile](https://github.com/rsmiller/KosmosERP/blob/main/Dockerfile) removes the appsettings.json file when compiled, so you will need to use environment variables in your host. Please see the configuration section for those variables
