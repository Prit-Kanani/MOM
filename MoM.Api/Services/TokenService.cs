using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace MoM.Api.Services
{
    public class TokenService
    {
        private readonly byte[] _secretKey;

        public TokenService(IConfiguration configuration)
        {
            var secret = configuration["Auth:Secret"];
            if (string.IsNullOrWhiteSpace(secret))
            {
                throw new InvalidOperationException("Auth:Secret configuration is required.");
            }

            _secretKey = Encoding.UTF8.GetBytes(secret);
        }

        public (string Token, DateTime ExpiresAtUtc) CreateToken(int userId, string userName)
        {
            var expiresAtUtc = DateTime.UtcNow.AddHours(12);
            var payload = $"{userId}|{userName}|{expiresAtUtc.Ticks}";
            var payloadBytes = Encoding.UTF8.GetBytes(payload);
            var signatureBytes = Sign(payloadBytes);

            var token = $"{WebEncoders.Base64UrlEncode(payloadBytes)}.{WebEncoders.Base64UrlEncode(signatureBytes)}";
            return (token, expiresAtUtc);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            var parts = token.Split('.', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                return null;
            }

            byte[] payloadBytes;
            byte[] signatureBytes;

            try
            {
                payloadBytes = WebEncoders.Base64UrlDecode(parts[0]);
                signatureBytes = WebEncoders.Base64UrlDecode(parts[1]);
            }
            catch
            {
                return null;
            }

            var expectedSignature = Sign(payloadBytes);
            if (!CryptographicOperations.FixedTimeEquals(signatureBytes, expectedSignature))
            {
                return null;
            }

            var payload = Encoding.UTF8.GetString(payloadBytes);
            var segments = payload.Split('|', 3);
            if (segments.Length != 3 ||
                !int.TryParse(segments[0], out var userId) ||
                !long.TryParse(segments[2], out var expiryTicks))
            {
                return null;
            }

            var expiresAtUtc = new DateTime(expiryTicks, DateTimeKind.Utc);
            if (expiresAtUtc <= DateTime.UtcNow)
            {
                return null;
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, segments[1]),
                new Claim("access_token_expires", expiresAtUtc.ToString("O"))
            };

            var identity = new ClaimsIdentity(claims, "MomBearer");
            return new ClaimsPrincipal(identity);
        }

        private byte[] Sign(byte[] payloadBytes)
        {
            using var hmac = new HMACSHA256(_secretKey);
            return hmac.ComputeHash(payloadBytes);
        }
    }
}
