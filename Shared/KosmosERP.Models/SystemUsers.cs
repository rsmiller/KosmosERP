namespace KosmosERP.Models;

/// <summary>
/// Built-in accounts. Audit fields (created_by, updated_by, ...) hold a user's guid; writes made
/// by the system itself (startup seeding, background jobs, automatic postings) are stamped with
/// the service account's guid.
/// </summary>
public static class SystemUsers
{
    /// <summary>
    /// Username of the service account. It is created disabled with an unusable password, so it
    /// can never sign in; it only exists so system writes reference a real user.
    /// </summary>
    public const string ServiceUsername = "kosmos-service";

    /// <summary>
    /// Fixed guid of the service account. Knowing it grants nothing: the API never takes the
    /// caller's identity from a request, only from the validated token.
    /// </summary>
    public const string ServiceUserGuid = KosmosERP.Database.SeedUsers.ServiceUserGuid;
}
