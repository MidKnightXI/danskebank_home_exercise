using DanskeBank.Communication.Services;

namespace DanskeBank.Communication.Tests
{
    public class PasswordHasherTests
    {
        [Fact]
        public void HashPassword_And_VerifyPassword_Success()
        {
            var password = "MySecretPassword!";
            var hash = PasswordHasher.HashPassword(password);
            Assert.True(PasswordHasher.VerifyPassword(password, hash));
        }

        [Fact]
        public void VerifyPassword_Fails_With_WrongPassword()
        {
            var password = "MySecretPassword!";
            var wrongPassword = "WrongPassword";
            var hash = PasswordHasher.HashPassword(password);
            Assert.False(PasswordHasher.VerifyPassword(wrongPassword, hash));
        }
    }
}
