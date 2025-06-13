using System.Text.Json.Serialization;

namespace DanskeBank.Communication.Models.Responses;

public class CustomerResponse: BaseResponse
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Customer? Customer { get; set; }
}