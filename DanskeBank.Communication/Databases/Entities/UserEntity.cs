using DanskeBank.Communication.Models;

namespace DanskeBank.Communication.Databases.Entities;

public class UserEntity : User
{
    public required Guid Id { get; set; }
}