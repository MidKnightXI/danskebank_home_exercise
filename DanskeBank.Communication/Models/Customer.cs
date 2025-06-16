using System.ComponentModel.DataAnnotations;

namespace DanskeBank.Communication.Models;

public class Customer
{
    [MinLength(2), MaxLength(64)]
    public required string Name { get; set; }
    [EmailAddress, MaxLength(320)]
    public required string Email { get; set; }
}