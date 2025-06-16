namespace DanskeBank.Communication.Models.Responses;

public class PaginatedResponse : BaseResponse
{
    public string? Next { get; set; }
    public string? Previous { get; set; }
    public int TotalItems { get; set; } = 0;
}
