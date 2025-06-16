using DanskeBank.Communication.Databases;
using DanskeBank.Communication.Databases.Entities;
using DanskeBank.Communication.Models;
using DanskeBank.Communication.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DanskeBank.Communication.Repositories;

public class TemplateRepository : ITemplateRepository
{
    private readonly CommunicationDbContext _dbContext;

    public TemplateRepository(CommunicationDbContext context)
    {
        _dbContext = context;
    }

    public async Task<TemplateEntity> AddAsync(Template template)
    {
        var templateEntity = new TemplateEntity
        {
            Id = Guid.NewGuid(),
            Name = template.Name,
            Subject = template.Subject,
            Body = template.Body,
        };

        await _dbContext.Templates.AddAsync(templateEntity);
        await _dbContext.SaveChangesAsync();
        _dbContext.Entry(templateEntity).State = EntityState.Detached;
        return templateEntity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var templateEntity = await _dbContext.Templates.FindAsync(id)
            ?? throw new KeyNotFoundException($"Template with ID {id} not found.");
        _dbContext.Templates.Remove(templateEntity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<TemplateEntity> GetByIdAsync(Guid id)
    {
        return await _dbContext.Templates.FindAsync(id)
            ?? throw new KeyNotFoundException($"Template with ID {id} not found.");
    }

    public async Task<List<TemplateEntity>> SearchAsync(string query)
    {
        return await _dbContext.Templates.AsNoTracking()
            .Where(t => t.Name.Contains(query) || t.Subject.Contains(query) || t.Body.Contains(query))
            .ToListAsync();
    }

    public async Task<TemplateEntity> UpdateAsync(Guid id, Template template)
    {
        var templateEntity = await _dbContext.Templates.FindAsync(id)
            ?? throw new KeyNotFoundException($"Template with ID {id} not found.");
        templateEntity.Name = template.Name;
        templateEntity.Subject = template.Subject;
        templateEntity.Body = template.Body;

        _dbContext.Templates.Update(templateEntity);
        await _dbContext.SaveChangesAsync();
        _dbContext.Entry(templateEntity).State = EntityState.Detached;
        return templateEntity;
    }

    public async Task<(List<TemplateEntity> Items, int TotalCount)> ListPaginatedAsync(int page, int pageSize)
    {
        var query = _dbContext.Templates.AsNoTracking();
        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, totalCount);
    }
}