# Prometheus ERP

## Warning 
This is a side project I sometimes work on. Sorry for the sporadic nature of it. This is currently not in a finished state, but feel free to use it to your desires.

## Use Case
This design is based on a few designs I used in the past.
The point of this project is to give anyone in the world who needs an internal ERP system the ability to create one from a generally available template, so they don't have to write one from scratch.

## Tech and Architecture
- RabbitMQ
- MySQL
- Entity Framework Core w/ Migrations
- Monolith for small business needs
- Run in Azure Web App, AKS, a VM, IIS, whatever

## Current State
As of 4/13/2025
- Database models are 95% of what an average company will need
- I started storage account factories that will support local and cloud systems. Details needed and testing.
- Created a messaging factory for providers like service bus, rabbitmq, pub/sub, ect.
- The following modules need to be created for a Minimal Viable Product: MRP, BOM, Production/Release Orders, Inventory, Return.
- Started development on the Api monolith.

As of 3/9/2025
- Database models are 90% of what an average company will need
- Database is currently configured for MySQL to save costs. Swap our builder.Services.AddDbContext in Program.cs. Configurations may need to be changed in Shared\Prometheus.Database\Configurations if errors are thrown because og Guid mismatch.
- Migrations are available for use. Look in Shared\Prometheus.Database for commands
- Unit tests are functional and will catch general items
- Started development on the Api monolith.

## Current MVP needs
- Finish API controllers and modules
- Unit tests for remaining modules (most)
- Testing all message providers
- Testing all storage account providers
- Redis integration
- Worker for inventory work
- Worker to process transactions

## Road Map
- Kubernetes scripts
- Compose script
- Certificates Module
- Quality/RMA Module
- Camera Module - use case: taking a picture of products on scales/pallets when receiving or when completed.
- 
