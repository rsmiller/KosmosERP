namespace KosmosERP.Database;

/// <summary>
/// User guids needed by EF seed data (HasData). This project can't reference
/// KosmosERP.Models, so the value lives here; code should use <c>SystemUsers</c>.
/// </summary>
public static class SeedUsers
{
    /// <summary>The system service account's <c>User.guid</c> (see <c>SystemUsers.ServiceUserGuid</c>).</summary>
    public const string ServiceUserGuid = "6b6f736d-6f73-4000-8000-000000000001";
}
