using DanskeBank.Communication.Databases.Entities;
using DanskeBank.Communication.Models.Dtos;

namespace DanskeBank.Communication.Extensions;

public static class UserMapper
{
    public static UserDto ToDto(this UserEntity entity)
    {
        return new UserDto
        {
            Id = entity.Id,
            Email = entity.Email
        };
    }

    public static List<UserDto> ToDtoList(this IEnumerable<UserEntity> entities)
    {
        return entities.Select(e => e.ToDto()).ToList();
    }
}
