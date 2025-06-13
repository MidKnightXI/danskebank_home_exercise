namespace DanskeBank.Communication.Models.Dtos;

public class TemplateDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Subject { get; set; }
    public required string Body { get; set; }
}
