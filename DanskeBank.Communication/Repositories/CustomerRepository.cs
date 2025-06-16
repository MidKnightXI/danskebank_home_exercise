using DanskeBank.Communication.Databases;
using DanskeBank.Communication.Databases.Entities;
using DanskeBank.Communication.Models;
using DanskeBank.Communication.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DanskeBank.Communication.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly CommunicationDbContext _dbContext;

    public CustomerRepository(CommunicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CustomerEntity> AddAsync(Customer customer, CancellationToken cancellationToken)
    {
        var customerEntity = new CustomerEntity
        {
            Id = Guid.NewGuid(),
            Name = customer.Name,
            Email = customer.Email
        };
        await _dbContext.Customers.AddAsync(customerEntity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _dbContext.Entry(customerEntity).State = EntityState.Detached;
        return customerEntity;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var customerEntity = await _dbContext.Customers.FindAsync(new object[] { id }, cancellationToken)
            ?? throw new KeyNotFoundException($"Customer with ID {id} not found.");
        _dbContext.Customers.Remove(customerEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<CustomerEntity> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Customer with ID {id} not found.");
    }

    public async Task<List<CustomerEntity>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        return await _dbContext.Customers.AsNoTracking()
            .Where(c => c.Name.Contains(query) || c.Email.Contains(query))
            .ToListAsync(cancellationToken);
    }

    public async Task<CustomerEntity> UpdateAsync(Guid id, Customer customer, CancellationToken cancellationToken)
    {
        var customerEntity = await _dbContext.Customers.FindAsync(new object[] { id }, cancellationToken)
            ?? throw new KeyNotFoundException($"Customer with ID {id} not found.");
        customerEntity.Name = customer.Name;
        customerEntity.Email = customer.Email;

        _dbContext.Customers.Update(customerEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _dbContext.Entry(customerEntity).State = EntityState.Detached;
        return customerEntity;
    }

    public async Task<(List<CustomerEntity> Items, int TotalCount)> ListPaginatedAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbContext.Customers.AsNoTracking();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }
}