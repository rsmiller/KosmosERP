# Prometheus ERP

## Warning 
This is a side project I sometimes work on. Sorry for the sporadic nature of it. This is currently not in a finished state but feel free to use it to your own desires.

## Use Case
This design is based on a few designs I used in the past.
The point of this project is to give anyone in the world who needs an internal ERP system the ability to create one from a generally available template, so they don't have to write one from complete scratch.

## Tech and Architecture
- RabbitMQ
- MySQL
- Entity Framework Core w/ Migrations
- Monolith for small business needs
- Run in am Azure Web App, AKS, a VM, IIS, whatever

## Current State
As of 3/9/2025
- Database models are 90% of what an average company will need
- Database is currently configured for MySQL to save costs. Swap our builder.Services.AddDbContext in Program.cs. Configurations may need to change in Shared\Prometheus.Database\Configurations if errors are thrown because og Guid mismatch.
- Migrations are available for use. Look in Shared\Prometheus.Database for commands
- Unit tests are functional and will catch general items
- Started development on the Api monolith.

## Current needs
- Finish API controllers and modules
- Kubernetes scripts
- Docker files
- Compose script