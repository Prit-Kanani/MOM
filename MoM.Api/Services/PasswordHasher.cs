using System.Security.Cryptography;
using System.Text;

namespace MoM.Api.Services
{
    public class PasswordHasher
    {
        public string GenerateSalt()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        }

        public string HashPassword(string password, string salt)
        {
            var saltBytes = Convert.FromBase64String(salt);
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var combined = new byte[saltBytes.Length + passwordBytes.Length];

            Buffer.BlockCopy(saltBytes, 0, combined, 0, saltBytes.Length);
            Buffer.BlockCopy(passwordBytes, 0, combined, saltBytes.Length, passwordBytes.Length);

            return Convert.ToBase64String(SHA256.HashData(combined));
        }

        public bool VerifyPassword(string password, string salt, string expectedHash)
        {
            var actualHash = HashPassword(password, salt);
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(actualHash),
                Convert.FromBase64String(expectedHash));
        }
    }
}
