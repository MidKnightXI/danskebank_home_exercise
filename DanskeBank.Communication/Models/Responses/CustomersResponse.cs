using System.Text.Json.Serialization;
using DanskeBank.Communication.Databases.Entities;

namespace DanskeBank.Communication.Models.Responses;

public class CustomersResponse : BaseResponse
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<CustomerEntity>? Customers { get; set; }
}