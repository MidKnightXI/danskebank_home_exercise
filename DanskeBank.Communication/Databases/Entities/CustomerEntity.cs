using DanskeBank.Communication.Models;

namespace DanskeBank.Communication.Databases.Entities;

public class CustomerEntity : Customer
{
    public required Guid Id { get; set; }
}