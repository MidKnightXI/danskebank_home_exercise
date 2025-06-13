using System.Text.Json.Serialization;
using DanskeBank.Communication.Models.Dtos;

namespace DanskeBank.Communication.Models.Responses;

public class CustomersResponse : BaseResponse
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<CustomerDto>? Customers { get; set; }
}