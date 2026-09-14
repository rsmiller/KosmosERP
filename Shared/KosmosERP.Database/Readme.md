-- EF Commands
dotnet ef migrations add XXXX --project Shared\KosmosERP.Database --context ERPDbContext
dotnet ef database update --project Shared\KosmosERP.Database --context ERPDbContext