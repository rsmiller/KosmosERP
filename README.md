# Prometheus ERP

## Warning 
This is a side project I sometimes work on. Sorry for the sporadic nature of it. This is currently not in a finished state, but feel free to use it to your desires.

## Use Case
- This design is based on a few designs I used in the past.
- The point of this project is to give anyone in the world who needs an internal ERP system the ability to create one from a generally available template, so they don't have to write one from scratch. I have a gut feeling there are going to be a lot of manufacturing start-ups in the next few years and jumping into SAP/Oracle/Info is cost prohibitive.
- This system does not, and should not be utilized, as an accounting system. It does not accept payments, wires, credit cards, ect. The generic accounting data in this ERP should be piped into Quickbooks, Zero, Wave, or FreshBooks because those systems accept payments in a controlled and compliant manner. This ERP enforces process control, allows for tracability of data, inventory counts, and reporting. Not for accounting.

## Tech and Architecture
- RabbitMQ, but will support major message queue systems
- Azure Blog Storage, but will support major store providers including local.
- MySQL
- Entity Framework Core w/ Migrations
- Monolith for small business needs
- Run in Azure Web App, AKS, a VM, IIS, whatever

## Current State
As of 4/24/2025
- Inventory/MRP tables, DTOs, services, and worker needs to be created.
- Need to create unit tests for all the services, which will undoubtedly require fixes and opportunities for improvement.
- I am putting return on hold because you can technically do this through inventory adjustments atm.
- I am looking to leverage Apache Superset for the BI dashboards and reports (https://superset.apache.org/) to be cost effective as possible while giving the end user a lot of control.

As of 4/13/2025
- Database models are 95% of what an average company will need
- I started storage account factories that will support local and cloud systems. Details needed and testing.
- Created a messaging factory for providers like service bus, rabbitmq, pub/sub, ect.
- The following modules need to be created for a Minimal Viable Product: MRP, BOM, Production/Release Orders, Inventory, Return.
- SignalR hub for notifications has been made but it may need some slight rework.

As of 3/9/2025
- Database models are 90% of what an average company will need
- Database is currently configured for MySQL to save costs. Swap our builder.Services.AddDbContext in Program.cs. Configurations may need to be changed in Shared\Prometheus.Database\Configurations if errors are thrown because og Guid mismatch.
- Migrations are available for use. Look in Shared\Prometheus.Database for commands
- Unit tests are functional and will catch general items
- Started development on the Api monolith.

## Current MVP needs
- Finish API controllers and modules
- Unit tests for modules
- Testing all message providers
- Testing all storage account providers
- Worker for inventory work
- Worker to process transactions
- Create a dozen common reports for Apache Superset

## Road Map
- Kubernetes scripts
- Compose script
- Certificates Module
- Quality/RMA Module
- Camera Module - use case: taking a picture of products on scales/pallets when receiving or when completed.
- Comment Module - user case: for a running tally of internal communication of objects like orders, invoices, production orders, ect.
