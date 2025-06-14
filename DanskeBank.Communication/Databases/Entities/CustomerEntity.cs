namespace DanskeBank.Communication.Databases.Entities;

public class CustomerEntity
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
}