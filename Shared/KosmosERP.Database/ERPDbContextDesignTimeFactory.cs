using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KosmosERP.Database;

/// <summary>
/// Lets the EF Core tools (<c>dotnet ef migrations add / database update</c>) create an
/// <see cref="ERPDbContext"/> without the API's service provider. Set
/// <c>KOSMOS_MIGRATIONS_CONNECTION</c> to target a database; <c>migrations add</c> doesn't
/// connect, so the localhost default is enough to generate a migration.
/// </summary>
public class ERPDbContextDesignTimeFactory : IDesignTimeDbContextFactory<ERPDbContext>
{
    public ERPDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("KOSMOS_MIGRATIONS_CONNECTION")
            ?? "server=localhost;port=3306;uid=root;pwd=;database=kosmos_erp";

        var options = new DbContextOptionsBuilder<ERPDbContext>()
            .UseMySQL(connection)
            .Options;

        return new ERPDbContext(options);
    }
}
