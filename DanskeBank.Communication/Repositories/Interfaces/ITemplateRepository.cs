using DanskeBank.Communication.Databases.Entities;
using DanskeBank.Communication.Models;
using System.Threading;

namespace DanskeBank.Communication.Repositories.Interfaces;

public interface ITemplateRepository
{
    Task<TemplateEntity> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<TemplateEntity> AddAsync(Template template, CancellationToken cancellationToken);
    Task<TemplateEntity> UpdateAsync(Guid id, Template template, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<List<TemplateEntity>> SearchAsync(string query, CancellationToken cancellationToken);
    Task<(List<TemplateEntity> Items, int TotalCount)> ListPaginatedAsync(int page, int pageSize, CancellationToken cancellationToken);
}