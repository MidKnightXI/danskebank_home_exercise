using Microsoft.EntityFrameworkCore;

using DanskeBank.Communication.Databases;
using DanskeBank.Communication.Databases.Entities;
using DanskeBank.Communication.Models;
using DanskeBank.Communication.Repositories.Interfaces;

namespace DanskeBank.Communication.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly CommunicationDbContext _dbContext;

    public CustomerRepository(CommunicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CustomerEntity> AddAsync(Customer customer)
    {
        var customerEntity = new CustomerEntity
        {
            Id = Guid.NewGuid(),
            Name = customer.Name,
            Email = customer.Email
        };
        await _dbContext.Customers.AddAsync(customerEntity);
        await _dbContext.SaveChangesAsync();
        _dbContext.Entry(customerEntity).State = EntityState.Detached;
        return customerEntity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var customerEntity = await _dbContext.Customers.FindAsync(id)
            ?? throw new KeyNotFoundException($"Customer with ID {id} not found.");
        _dbContext.Customers.Remove(customerEntity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<CustomerEntity>> ListAsync()
    {
        return await _dbContext.Customers.AsNoTracking().ToListAsync();
    }

    public async Task<CustomerEntity> GetByIdAsync(Guid id)
    {
        return await _dbContext.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Customer with ID {id} not found.");
    }

    public async Task<List<CustomerEntity>> SearchAsync(string query)
    {
        return await _dbContext.Customers.AsNoTracking()
            .Where(c => c.Name.Contains(query) || c.Email.Contains(query))
            .ToListAsync();
    }

    public async Task<CustomerEntity> UpdateAsync(Guid id, Customer customer)
    {
        var customerEntity = await _dbContext.Customers.FindAsync(id)
            ?? throw new KeyNotFoundException($"Customer with ID {id} not found.");
        customerEntity.Name = customer.Name;
        customerEntity.Email = customer.Email;

        _dbContext.Customers.Update(customerEntity);
        await _dbContext.SaveChangesAsync();
        _dbContext.Entry(customerEntity).State = EntityState.Detached;
        return customerEntity;
    }
}