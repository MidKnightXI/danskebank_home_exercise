using System.Net;
using System.Net.Mail;
using DanskeBank.Communication.Databases.Entities;

namespace DanskeBank.Communication.Services;

public class MailingService
{
    public bool Enabled { get; set; }
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPassword;

    public MailingService(string smtpServer, int smtpPort, string smtpUser, string smtpPassword, bool enabled = false)
    {
        _smtpServer = smtpServer;
        _smtpPort = smtpPort;
        _smtpUser = smtpUser;
        _smtpPassword = smtpPassword;
        Enabled = enabled;
    }

    private static string CensorEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return string.Empty;
        }
        var atIndex = email.IndexOf('@');
        if (atIndex < 2)
        {
            return email;
        }
        var censoredPart = new string('*', atIndex - 1);
        return $"{email[0]}{censoredPart}{email.Substring(atIndex)}";
    }

    public string FormatEmailBody(string body, CustomerEntity customer)
    {
        return body
            .Replace("{{Customer.Name}}", customer.Name)
            .Replace("{{Customer.Email}}", customer.Email)
            .Replace("{{Customer.CensoredEmail}}", CensorEmail(customer.Email))
            .Replace("{{Sender.Email}}", _smtpUser)
            .Replace("{{Date}}", DateTime.UtcNow.ToString("yyyy-MM-dd"));
    }

    public async Task SendEmailAsync(string subject, string body, string toEmail, CancellationToken cancellationToken = default)
    {
        if (Enabled is false)
        {
            return;
        }
        using var client = new SmtpClient(_smtpServer, _smtpPort)
        {
            Credentials = new NetworkCredential(_smtpUser, _smtpPassword),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_smtpUser),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        mailMessage.To.Add(toEmail);

        await client.SendMailAsync(mailMessage, cancellationToken);
    }
}