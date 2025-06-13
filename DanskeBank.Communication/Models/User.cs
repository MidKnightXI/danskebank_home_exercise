using System.Text.Json.Serialization;

namespace DanskeBank.Communication.Models;

public class User
{
    public required string Email { get; set; }
    [JsonIgnore]
    public required string Password { get; set; }
}