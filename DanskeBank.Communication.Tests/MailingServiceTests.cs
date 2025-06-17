using DanskeBank.Communication.Databases.Entities;
using DanskeBank.Communication.Services;

namespace DanskeBank.Communication.Tests
{
    public class MailingServiceTests
    {
        [Fact]
        public void FormatEmailBody_ReplacesAllVariablesCorrectly()
        {
            // Arrange
            var customer = new CustomerEntity { Id = Guid.NewGuid(), Name = "Alice", Email = "alice@example.com" };
            var service = new MailingService("smtp.example.com", 587, "sender@example.com", "password", true);
            var template = @"<p>Hello {{Customer.Name}},</p>\n<p>Your email: {{Customer.Email}}</p>\n<p>Censored: {{Customer.CensoredEmail}}</p>\n<p>From: {{Sender.Email}}</p>\n<p>Date: {{Date}}</p>";

            // Act
            var result = service.FormatEmailBody(template, customer);

            // Assert
            Assert.Contains("Hello Alice", result);
            Assert.Contains("Your email: alice@example.com", result);
            Assert.Contains("Censored: a****@example.com", result);
            Assert.Contains("From: sender@example.com", result);
            Assert.Contains(DateTime.UtcNow.ToString("yyyy-MM-dd"), result);
        }
    }
}
