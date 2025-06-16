using DanskeBank.Communication.Databases.Entities;
using DanskeBank.Communication.Extensions;
using DanskeBank.Communication.Models;
using DanskeBank.Communication.Models.Responses;
using DanskeBank.Communication.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DanskeBank.Communication.Controllers;

// this controller should be in a separate microservice if we were in a microservices architecture

[Authorize]
[ApiController]
[Route("api/v1/users")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UserController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedUsersResponse>> GetUsers(CancellationToken cancellationToken, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        try
        {
            var (users, totalCount) = await _userRepository.ListPaginatedAsync(page, pageSize, cancellationToken);
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            string? next = (page * pageSize < totalCount) ? $"{baseUrl}?page={page + 1}&pageSize={pageSize}" : null;
            string? previous = (page > 1) ? $"{baseUrl}?page={page - 1}&pageSize={pageSize}" : null;
            return Ok(new PaginatedUsersResponse
            {
                Success = true,
                Users = users.ToDtoList(),
                Count = totalCount,
                Next = next,
                Previous = previous
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new PaginatedUsersResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserEntity>> GetUser(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
            return Ok(new UserResponse
            {
                Success = true,
                User = user.ToDto()
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new UserResponse
            {
                Success = false,
                Message = $"User with ID {id} not found."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new UserResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<UserResponse>> CreateUser([FromBody] User user, CancellationToken cancellationToken)
    {
        try
        {
            var userEntity = await _userRepository.AddAsync(user, cancellationToken);
            return CreatedAtAction(nameof(GetUser), new { id = userEntity.Id }, new UserResponse
            {
                Success = true,
                User = userEntity.ToDto()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new UserResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserResponse>> UpdateUser(Guid id, [FromBody] User user, CancellationToken cancellationToken)
    {
        try
        {
            var userEntity = await _userRepository.UpdateAsync(id, user, cancellationToken);
            return Ok(new UserResponse
            {
                Success = true,
                User = userEntity.ToDto()
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new UserResponse
            {
                Success = false,
                Message = $"User with ID {id} not found."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new UserResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _userRepository.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new UserResponse
            {
                Success = false,
                Message = $"User with ID {id} not found."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new UserResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }
}