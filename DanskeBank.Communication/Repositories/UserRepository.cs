using DanskeBank.Communication.Databases;
using DanskeBank.Communication.Databases.Entities;
using DanskeBank.Communication.Models;
using DanskeBank.Communication.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DanskeBank.Communication.Repositories;

public class UserRepository : IUserRepository
{
    private readonly CommunicationDbContext _dbContext;

    public UserRepository(CommunicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(User user)
    {
        var userEntity = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = user.Email,
            Password = user.Password,
        };
        await _dbContext.Users.AddAsync(userEntity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var userEntity = await _dbContext.Users.FindAsync(id);
        if (userEntity is null)
        {
            return;
        }

        _dbContext.Users.Remove(userEntity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<UserEntity> GetByIdAsync(Guid id)
    {
        var userEntity = await _dbContext.Users.FindAsync(id);
        return userEntity ?? throw new KeyNotFoundException($"User with ID {id} not found.");
    }

    public async Task<List<UserEntity>> ListAsync()
    {
        return await _dbContext.Users.ToListAsync();
    }

    public async Task<List<UserEntity>> SearchAsync(string query)
    {
        return await _dbContext.Users
            .Where(u => u.Email.Contains(query))
            .ToListAsync();
    }

    public async Task UpdateAsync(Guid id, User user)
    {
        var userEntity = await _dbContext.Users.FindAsync(id);
        if (userEntity is null)
        {
            return;
        }

        userEntity.Email = user.Email;
        userEntity.Password = user.Password;

        _dbContext.Users.Update(userEntity);
        await _dbContext.SaveChangesAsync();
    }
}