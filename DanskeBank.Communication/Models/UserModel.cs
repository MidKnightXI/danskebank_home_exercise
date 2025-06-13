using System.Text.Json.Serialization;

namespace DanskeBank.Communication.Models;

public class UserModel
{
    public required Guid Id { get; set; }
    public required string Email { get; set; }
    [JsonIgnore]
    public required string Password { get; set; }
}