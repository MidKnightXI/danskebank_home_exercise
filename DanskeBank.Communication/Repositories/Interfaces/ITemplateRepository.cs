using DanskeBank.Communication.Databases.Entities;
using DanskeBank.Communication.Models;

namespace DanskeBank.Communication.Repositories.Interfaces;

public interface ITemplateRepository
{
    Task<TemplateEntity> GetByIdAsync(Guid id);
    Task<TemplateEntity> AddAsync(Template template);
    Task<TemplateEntity> UpdateAsync(Guid id, Template template);
    Task DeleteAsync(Guid id);
    Task<List<TemplateEntity>> SearchAsync(string query);
    Task<List<TemplateEntity>> ListAsync();
}