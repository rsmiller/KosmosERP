
# Kosmos ERP - Environmental Variables

 When installing depending on your design decisions you will be required to include environmental variables durring setup.

### Message Publisher
**Environmental Variable:** MessagePublisherAccountProvider
**Values:** database, azure, rabbitmq

    // IF Azure additional variables
    AzureBusConnectionString = SERVICE BUS CONNECTION STRING
    
    // If RabbitMq additional variables
    RabbitMQHost = THE URL BASE PATH
    RabbitMQUsername = USERNAME
    RabbitMQPassword = PASSWORD
    RabbitMQPort = PORT NUMBER
    RabbitMQVirtualHost = VIRTUAL HOST IF USED
    RabbitMQExchange = EXCHANGE
  
---
### Log Provider:
**Environmental Variable:** LogProvider
**Values:** azure, datadog, database

    // If Azure additional variables
    ApplicationInsightsConnectionString = INSIGHTS CONNECTION STRING
    
    // If Datadog additional variables
    DD_API_KEY = YOUR API KEY
    DD_SITE = THE DATADOG INJECTION URL
    DD_ENV = YOUR ENVIRONMENT DEV OR PROD
    DD_LOGS_INJECTION = true
    DD_LOGS_DIRECT_SUBMISSION_INTEGRATIONS = Serilog
    
---

### File Storage Type
**Environmental Variable:** FileStorageAccountProvider
**Values:** local, azure, aws

    // If local additional variables
    LocalStoragePath = FILE PATH TO SERVER FOLDER, OR MSA
    
    // If Azure additional variables
    AzureStorageConnectionString = STORAGE URL CONNECTION STRING
    AzureContainerName = AZURE CONTAINER NAME
    
    // If AWS
    AWSRegion = REGION LIKE us-east-1
    AWSBucketName = BUCKET NAME
    AWSAccessKey = YOUR ACCESS KEY
    AWSSecretKey = ACCESS KEY SECRET

### Authentication Provder:
**Environmental Variable:** AuthenticationProvider
**Values:** keycloak

    IF Keycloak additional variables
    Realm = YOUR REALM NAME
    BaseURL = THE BASE URL LIKE https://yoururl.com/
    Authority = URL TO REALM LIKE http://yoururl.com:37167/realms/kosmitech"
    Audience = THE AUDIENCE
    TokenURL = TOKEN URL LIKE http://yoururl.com:37167/realms/YOUREALM/protocol/openid-connect/token
    ClientSecret = THE CLIENT SECRET

---

### Payment Provder:
**Environmental Variable:** PaymentProvider
**Values:** stripe

    // If Stripe additional variables
    StripeApiKey = STRIPE API KEY
