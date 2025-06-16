using System.ComponentModel.DataAnnotations;

namespace DanskeBank.Communication.Models;

public class User
{
    [EmailAddress, MaxLength(320)]
    public required string Email { get; set; }
    [MinLength(6), MaxLength(32)]
    public required string Password { get; set; }
}