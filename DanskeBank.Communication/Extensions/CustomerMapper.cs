using DanskeBank.Communication.Databases.Entities;
using DanskeBank.Communication.Models.Dtos;

namespace DanskeBank.Communication.Extensions;

public static class CustomerMapper
{
    public static CustomerDto ToDto(this CustomerEntity entity)
    {
        return new CustomerDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Email = entity.Email
        };
    }

    public static List<CustomerDto> ToDtoList(this IEnumerable<CustomerEntity> entities)
    {
        return entities.Select(e => e.ToDto()).ToList();
    }
}
