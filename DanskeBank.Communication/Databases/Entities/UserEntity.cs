namespace DanskeBank.Communication.Databases.Entities;

public class UserEntity
{
    public required Guid Id { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}