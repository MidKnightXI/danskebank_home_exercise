namespace DanskeBank.Communication.Models.Responses;

public class BaseResponse
{
    public required bool Success { get; set; }
    public string? Message { get; set; }
}