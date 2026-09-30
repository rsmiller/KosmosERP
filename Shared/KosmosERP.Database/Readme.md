-- EF Commands
dotnet tool install --global dotnet-ef
dotnet ef migrations add XXXX --project Shared\KosmosERP.Database --context ERPDbContext
dotnet ef database update --project Shared\KosmosERP.Database --context ERPDbContext

-- Connection
The EF tools create the context through ERPDbContextDesignTimeFactory, which reads the
KOSMOS_MIGRATIONS_CONNECTION environment variable, e.g.

    set KOSMOS_MIGRATIONS_CONNECTION=server=localhost;port=3306;uid=...;pwd=...;database=kosmos_erp

`migrations add` doesn't connect, so no variable is needed to generate a migration.
`database update` and `migrations remove` do connect.

-- Notes
- Seed data (HasData) must be deterministic: use fixed dates, never DateTime.Now/UtcNow,
  or every new migration will rewrite the seeded rows.
- MySQL won't MODIFY a column that other tables reference by foreign key while FK checks
  are on. See 20260930225836_FixOrderHeaderPaymentRelationship for the pattern.
