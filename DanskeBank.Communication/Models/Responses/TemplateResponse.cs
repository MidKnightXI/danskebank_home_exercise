using System.Text.Json.Serialization;
using DanskeBank.Communication.Models.Dtos;

namespace DanskeBank.Communication.Models.Responses;

public class TemplateResponse : BaseResponse
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TemplateDto? Template { get; set; }
}