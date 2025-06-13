namespace DanskeBank.Communication.Models;

public class CustomerModel
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
}