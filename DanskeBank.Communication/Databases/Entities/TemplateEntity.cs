using DanskeBank.Communication.Models;

namespace DanskeBank.Communication.Databases.Entities;

public class TemplateEntity : Template
{
    public required Guid Id { get; set; }
}