namespace DanskeBank.Communication.Databases.Entities;

public class TemplateEntity
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Subject { get; set; }
    public required string Body { get; set; }
}