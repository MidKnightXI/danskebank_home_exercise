using DanskeBank.Communication.Databases;
using DanskeBank.Communication.Databases.Entities;
using DanskeBank.Communication.Models;
using DanskeBank.Communication.Repositories.Interfaces;
using DanskeBank.Communication.Services;
using Microsoft.EntityFrameworkCore;

namespace DanskeBank.Communication.Repositories;

public class UserRepository : IUserRepository
{
    private readonly CommunicationDbContext _dbContext;

    public UserRepository(CommunicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserEntity> AddAsync(User user)
    {
        var userEntity = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = user.Email,
            Password = PasswordHasher.HashPassword(user.Password),
        };
        await _dbContext.Users.AddAsync(userEntity);
        await _dbContext.SaveChangesAsync();
        _dbContext.Entry(userEntity).State = EntityState.Detached;
        return userEntity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var userEntity = await _dbContext.Users.FindAsync(id)
            ?? throw new KeyNotFoundException($"User with ID {id} not found.");
        _dbContext.Users.Remove(userEntity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<UserEntity> GetByIdAsync(Guid id)
    {
        return await _dbContext.Users.FindAsync(id)
            ?? throw new KeyNotFoundException($"User with ID {id} not found.");
    }

    public async Task<List<UserEntity>> SearchAsync(string query)
    {
        return await _dbContext.Users.AsNoTracking()
            .Where(u => u.Email.Contains(query))
            .ToListAsync();
    }

    public async Task<UserEntity> UpdateAsync(Guid id, User user)
    {
        var userEntity = await _dbContext.Users.FindAsync(id)
            ?? throw new KeyNotFoundException($"User with ID {id} not found.");
        userEntity.Email = user.Email;
        userEntity.Password = PasswordHasher.HashPassword(user.Password);

        _dbContext.Users.Update(userEntity);
        await _dbContext.SaveChangesAsync();
        _dbContext.Entry(userEntity).State = EntityState.Detached;
        return userEntity;
    }

    public async Task<UserEntity?> GetByEmailAsync(string email)
    {
        return await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<(List<UserEntity> Items, int TotalCount)> ListPaginatedAsync(int page, int pageSize)
    {
        var query = _dbContext.Users.AsNoTracking();
        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, totalCount);
    }
}