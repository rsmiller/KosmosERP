using System.Text.Json;
using KosmosERP.Database;
using Microsoft.EntityFrameworkCore;

namespace KosmosERP.Seeder;

/// <summary>
/// Dev-only console seeder. Populates a DEV database with a realistic computer-parts
/// manufacturer dataset for reporting and testing. It never runs inside the API/prod path.
///
/// Usage:
///   dotnet run --project Tools/KosmosERP.Seeder -- --connection "server=...;database=...;user=...;password=..."
///
/// Connection string resolution order: --connection arg > SEED_CONNECTION env var > appsettings.json.
/// The database SCHEMA must already exist (run `dotnet ef database update --project Shared/KosmosERP.Database`).
/// The seeder is idempotent: if the seed dataset is already present it exits without changes.
/// </summary>
public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var connectionString = ResolveConnectionString(args);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.Error.WriteLine(
                "No connection string. Pass --connection \"<cs>\", set SEED_CONNECTION, or fill appsettings.json.");
            return 1;
        }

        var options = new DbContextOptionsBuilder<ERPDbContext>()
            .UseMySQL(connectionString)
            .Options;

        var reset = args.Any(a => string.Equals(a, "--reset", StringComparison.OrdinalIgnoreCase));

        try
        {
            using var context = new ERPDbContext(options);

            // Hold one connection open for the whole run with FK checks off. The app schema has a
            // few misconfigured foreign keys (e.g. FK_order_headers_payments_id points the wrong
            // way), which would otherwise make it impossible to insert perfectly valid rows. Our
            // insert order already satisfies the legitimate relationships, so this only bypasses
            // the broken constraints. The SET is session-scoped, so the connection must stay open.
            var conn = context.Database.GetDbConnection();
            await conn.OpenAsync();
            await ExecAsync(conn, "SET FOREIGN_KEY_CHECKS=0");
            try
            {
                var seeder = new DatabaseSeeder(context);
                if (reset)
                    await seeder.ResetAsync();
                await seeder.SeedAsync();
            }
            finally
            {
                try { await ExecAsync(conn, "SET FOREIGN_KEY_CHECKS=1"); } catch { /* best effort */ }
                await conn.CloseAsync();
            }
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Seeding failed: " + ex.Message);
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static async Task ExecAsync(System.Data.Common.DbConnection conn, string sql)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync();
    }

    private static string ResolveConnectionString(string[] args)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], "--connection", StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        }

        var fromEnv = Environment.GetEnvironmentVariable("SEED_CONNECTION");
        if (!string.IsNullOrWhiteSpace(fromEnv))
            return fromEnv;

        var appsettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (File.Exists(appsettingsPath))
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(appsettingsPath));
            if (doc.RootElement.TryGetProperty("DatabaseConnectionString", out var cs))
                return cs.GetString();
        }

        return null;
    }
}
