using DanskeBank.Communication.Databases;
using DanskeBank.Communication.Databases.Entities;
using DanskeBank.Communication.Models;
using DanskeBank.Communication.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DanskeBank.Communication.Repositories;

public class TemplateRepository : ITemplateRepository
{
    private readonly CommunicationDbContext _context;

    public TemplateRepository(CommunicationDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(Template template)
    {
        var templateEntity = new TemplateEntity
        {
            Id = Guid.NewGuid(),
            Name = template.Name,
            Subject = template.Subject,
            Body = template.Body,
        };

        _context.Templates.Add(templateEntity);
        return _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var templateEntity = await _context.Templates.FindAsync(id)
            ?? throw new KeyNotFoundException($"Template with ID {id} not found.");
        _context.Templates.Remove(templateEntity);
        await _context.SaveChangesAsync();
    }

    public async Task<TemplateEntity> GetByIdAsync(Guid id)
    {
        return await _context.Templates.FindAsync(id)
            ?? throw new KeyNotFoundException($"Template with ID {id} not found.");
    }

    public async Task<List<TemplateEntity>> ListAsync()
    {
        return await _context.Templates.AsNoTracking().ToListAsync();
    }

    public async Task<List<TemplateEntity>> SearchAsync(string query)
    {
        return await _context.Templates.AsNoTracking()
            .Where(t => t.Name.Contains(query) || t.Subject.Contains(query) || t.Body.Contains(query))
            .ToListAsync();
    }

    public async Task UpdateAsync(Guid id, Template template)
    {
        var templateEntity = await _context.Templates.FindAsync(id)
            ?? throw new KeyNotFoundException($"Template with ID {id} not found.");
        templateEntity.Name = template.Name;
        templateEntity.Subject = template.Subject;
        templateEntity.Body = template.Body;

        _context.Templates.Update(templateEntity);
        await _context.SaveChangesAsync();
    }
}