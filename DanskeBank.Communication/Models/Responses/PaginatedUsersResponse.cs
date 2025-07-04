using System.Text.Json.Serialization;
using DanskeBank.Communication.Models.Dtos;

namespace DanskeBank.Communication.Models.Responses;

public class PaginatedUsersResponse : PaginatedResponse
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<UserDto>? Users { get; set; }
}
