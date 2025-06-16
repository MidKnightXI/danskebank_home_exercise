using DanskeBank.Communication.Models;
using DanskeBank.Communication.Models.Responses;
using DanskeBank.Communication.Repositories.Interfaces;
using DanskeBank.Communication.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.IdentityModel.Tokens.Jwt;

// this controller should also be in a separate microservice if we were in a microservices architecture

namespace DanskeBank.Communication.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly JwtService _jwtService;

    public AuthController(IUserRepository userRepository, JwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    [EnableRateLimiting("LoginPolicy")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] User user, CancellationToken cancellationToken)
    {
        try
        {

            var userEntity = await _userRepository.GetByEmailAsync(user.Email, cancellationToken);
            if (userEntity is null || PasswordHasher.VerifyPassword(user.Password, userEntity.Password) is false)
            {
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    Message = "Invalid credentials"
                });
            }
            var (token, expiresAt) = _jwtService.GenerateToken(userEntity.Id.ToString(), userEntity.Email);
            var (refreshToken, refreshTokenExpiresAt) = _jwtService.GenerateRefreshToken(userEntity.Id.ToString(), userEntity.Email);
            return Ok(new LoginResponse
            {
                Success = true,
                Token = token,
                ExpiresAt = expiresAt,
                RefreshToken = refreshToken,
                RefreshTokenExpiresAt = refreshTokenExpiresAt
            });
        }
        catch (KeyNotFoundException)
        {
            return Unauthorized(new LoginResponse
            {
                Success = false,
                Message = "User not found"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new LoginResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpPost("refresh")]
    [EnableRateLimiting("RefreshTokenPolicy")]
    public ActionResult<LoginResponse> Refresh([FromBody] string refreshToken, CancellationToken cancellationToken)
    {
        try
        {
            var jwt = _jwtService.ValidateRefreshToken(refreshToken);
            if (jwt is null)
            {
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    Message = "Invalid refresh token"
                });
            }
            var userId = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            var email = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
            if (userId is null || email is null)
            {
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    Message = "Invalid refresh token"
                });
            }
            var (token, expiresAt) = _jwtService.GenerateToken(userId, email);
            var (newRefreshToken, newRefreshTokenExpiresAt) = _jwtService.GenerateRefreshToken(userId, email);
            return Ok(new LoginResponse
            {
                Success = true,
                Token = token,
                ExpiresAt = expiresAt,
                RefreshToken = newRefreshToken,
                RefreshTokenExpiresAt = newRefreshTokenExpiresAt
            });
        }
        catch (Exception ex)
        {
            return Unauthorized(new LoginResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }
}
