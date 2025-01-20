-- EF Commands
dotnet ef migrations add XXXX --project Shared\Prometheus.Database --context ERPDbContext
dotnet ef database update --project Shared\Prometheus.Database --context ERPDbContext