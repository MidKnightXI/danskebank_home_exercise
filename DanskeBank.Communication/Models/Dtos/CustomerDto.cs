namespace DanskeBank.Communication.Models.Dtos;

public class CustomerDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
}
