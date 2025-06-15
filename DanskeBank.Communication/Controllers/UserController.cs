using DanskeBank.Communication.Databases.Entities;
using DanskeBank.Communication.Extensions;
using DanskeBank.Communication.Models;
using DanskeBank.Communication.Models.Responses;
using DanskeBank.Communication.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DanskeBank.Communication.Controllers;

// technically, this controller should be in a separate microservice if we were in a microservices architecture

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
    public async Task<ActionResult<UsersResponse>> GetUsers()
    {
        try
        {
            var users = await _userRepository.ListAsync();
            return Ok(new UsersResponse
            {
                Success = true,
                Users = users.ToDtoList(),
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new UsersResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserEntity>> GetUser(Guid id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
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
    public async Task<ActionResult<UserResponse>> CreateUser([FromBody] User user)
    {
        try
        {
            var userEntity = await _userRepository.AddAsync(user);
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
    public async Task<ActionResult<UserResponse>> UpdateUser(Guid id, [FromBody] User user)
    {
        try
        {
            var userEntity = await _userRepository.UpdateAsync(id, user);
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
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        try
        {
            await _userRepository.DeleteAsync(id);
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