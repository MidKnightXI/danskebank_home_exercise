namespace DanskeBank.Communication.Models.Responses;

public class LoginResponse : BaseResponse
{
    public string? Token { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }
}
