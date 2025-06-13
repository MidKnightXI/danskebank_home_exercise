namespace DanskeBank.Communication.Models;

public class TemplateModel
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Subject { get; set; }
    public required string Body { get; set; }
}