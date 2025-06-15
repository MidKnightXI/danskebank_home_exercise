using DanskeBank.Communication.Services;
using System.IdentityModel.Tokens.Jwt;

namespace DanskeBank.Communication.Tests
{
    public class JwtServiceTests
    {
        private static JwtService GetJwtService()
        {
            return new JwtService("supersecretkey1234567890abcdef12345678", "issuer", "audience", 1, 1);
        }

        [Fact]
        public void GenerateToken_And_ValidateClaims_Success()
        {
            var jwtService = GetJwtService();
            var userId = Guid.NewGuid().ToString();
            var email = "user@example.com";
            var (token, expiresAt) = jwtService.GenerateToken(userId, email);
            Assert.False(string.IsNullOrEmpty(token));
            Assert.True(expiresAt > DateTime.UtcNow);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            Assert.Equal(userId, jwt.Payload[JwtRegisteredClaimNames.Sub]);
            Assert.Equal(email, jwt.Payload[JwtRegisteredClaimNames.Email]);
        }

        [Fact]
        public void GenerateRefreshToken_And_ValidateClaims_Success()
        {
            var jwtService = GetJwtService();
            var userId = Guid.NewGuid().ToString();
            var email = "user@example.com";
            var (refreshToken, expiresAt) = jwtService.GenerateRefreshToken(userId, email);
            Assert.False(string.IsNullOrEmpty(refreshToken));
            Assert.True(expiresAt > DateTime.UtcNow);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(refreshToken);
            Assert.Equal(userId, jwt.Payload[JwtRegisteredClaimNames.Sub]);
            Assert.Equal(email, jwt.Payload[JwtRegisteredClaimNames.Email]);
            Assert.Equal("refresh", jwt.Payload["typ"]);
        }
    }
}
