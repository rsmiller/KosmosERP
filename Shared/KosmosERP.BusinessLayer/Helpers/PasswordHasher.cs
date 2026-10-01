using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace KosmosERP.BusinessLayer.Helpers
{
    /// <summary>
    /// Database-login passwords: PBKDF2 (HMAC-SHA1, 10,000 iterations, 32-byte hash)
    /// with a random 16-byte salt, both stored Base64 in users.password / password_salt.
    /// UserModule (create, password change), DatabaseAuthenticationProvider (login) and
    /// the dev seeder all go through here, so they can't drift apart (BUG-009).
    /// </summary>
    public static class PasswordHasher
    {
        /// <summary>A new random salt and the password's hash with it.</summary>
        public static (string Hash, string Salt) Create(string password)
        {
            var salt = new byte[128 / 8];
            RandomNumberGenerator.Fill(salt);

            return (Hash(password, salt), Convert.ToBase64String(salt));
        }

        /// <summary>
        /// Whether <paramref name="password"/> matches a stored hash and salt. A salt that
        /// isn't Base64 (bad or hand-written data) is a mismatch, not an exception.
        /// </summary>
        public static bool Verify(string password, string storedHash, string storedSalt)
        {
            byte[] salt;
            try
            {
                salt = Convert.FromBase64String(storedSalt ?? "");
            }
            catch (FormatException)
            {
                return false;
            }

            var actual = Convert.FromBase64String(Hash(password, salt));
            byte[] expected;
            try
            {
                expected = Convert.FromBase64String(storedHash ?? "");
            }
            catch (FormatException)
            {
                return false;
            }

            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }

        private static string Hash(string password, byte[] salt)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
        }
    }
}
