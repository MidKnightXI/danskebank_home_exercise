using System.Text.Json.Serialization;
using DanskeBank.Communication.Models.Dtos;

namespace DanskeBank.Communication.Models.Responses;

public class CustomerResponse: BaseResponse
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CustomerDto? Customer { get; set; }
}