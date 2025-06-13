using DanskeBank.Communication.Databases.Entities;
using Microsoft.AspNetCore.Mvc;

namespace DanskeBank.Communication.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UserController : ControllerBase
{
    [HttpGet]
    public ActionResult<List<UserEntity>> GetUsers()
    {
        return Ok(new List<UserEntity>());
    }

    [HttpGet("{id}")]
    public ActionResult<UserEntity> GetUser(Guid id)
    {
        return Ok(new UserEntity { Id = id, Email = "user@example.com", Password = "securepassword" });
    }

    [HttpPost]
    public ActionResult<UserEntity> CreateUser([FromBody] UserEntity user)
    {
        if (user == null || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
        {
            return BadRequest("Invalid user data.");
        }
        user.Id = Guid.NewGuid();
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public ActionResult<UserEntity> UpdateUser(Guid id, [FromBody] UserEntity user)
    {
        if (user == null || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
        {
            return BadRequest("Invalid user data.");
        }

        user.Id = id;
        return Ok(user);
    }

    [HttpDelete("{id}")]
    public ActionResult DeleteUser(Guid id)
    {
        return NoContent();
    }
}