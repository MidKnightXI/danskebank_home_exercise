using System.Text.Json.Serialization;
using DanskeBank.Communication.Models.Dtos;

namespace DanskeBank.Communication.Models.Responses;

public class TemplatesResponse : BaseResponse
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<TemplateDto>? Templates { get; set; }
}