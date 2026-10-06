using KosmosERP.Database;
using Microsoft.EntityFrameworkCore;

namespace KosmosERP.BusinessLayer.Helpers
{
    public static class UserNameHelper
    {
        /// <summary>
        /// Resolves an audit value (created_by, updated_by, ...), which holds the user's
        /// <c>User.guid</c>, to the user's "first last" name. Null when no such user exists.
        /// </summary>
        public static async Task<string?> GetFullName(IBaseERPContext context, string? user_guid)
        {
            if (string.IsNullOrEmpty(user_guid))
                return null;

            return await context.Users.Where(m => m.guid == user_guid).Select(m => m.first_name + " " + m.last_name).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Resolves a domain user reference that holds the database <c>User.id</c> as a string
        /// (Opportunity.owner_id) to the user's "first last" name.
        /// </summary>
        public static async Task<string?> GetFullNameById(IBaseERPContext context, string? user_id)
        {
            if (!int.TryParse(user_id, out var id))
                return null;

            return await context.Users.Where(m => m.id == id).Select(m => m.first_name + " " + m.last_name).SingleOrDefaultAsync();
        }
    }
}
