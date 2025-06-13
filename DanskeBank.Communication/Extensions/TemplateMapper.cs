using DanskeBank.Communication.Databases.Entities;
using DanskeBank.Communication.Models.Dtos;

namespace DanskeBank.Communication.Extensions;

public static class TemplateMapper
{
    public static TemplateDto ToDto(this TemplateEntity entity)
    {
        return new TemplateDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Subject = entity.Subject,
            Body = entity.Body
        };
    }

    public static List<TemplateDto> ToDtoList(this IEnumerable<TemplateEntity> entities)
    {
        return entities.Select(e => e.ToDto()).ToList();
    }
}
