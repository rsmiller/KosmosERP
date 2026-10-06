using KosmosERP.Database.Models;
using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Helpers
{
    public static class ServiceUserFactory
    {
        /// <summary>
        /// Builds the system service account (<see cref="SystemUsers.ServiceUserGuid"/>) that
        /// system writes are stamped with. It can never sign in: it is disabled (database login
        /// rejects it and the API's claims transformation skips it), its password is the hash
        /// of a random value nobody keeps, and it has no external id for Keycloak/SAML.
        /// </summary>
        public static User Create()
        {
            var (hash, salt) = PasswordHasher.Create(Guid.NewGuid().ToString() + Guid.NewGuid().ToString());

            return CommonDataHelper<User>.FillCommonFields(new User()
            {
                guid = SystemUsers.ServiceUserGuid,
                username = SystemUsers.ServiceUsername,
                first_name = "Kosmos",
                last_name = "Service",
                employee_number = "SYSTEM",
                password = hash,
                password_salt = salt,
                is_disabled = true,
                is_admin = false,
            }, SystemUsers.ServiceUserGuid);
        }
    }
}
