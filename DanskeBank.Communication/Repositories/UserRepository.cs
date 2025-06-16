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

    public async Task<UserEntity> AddAsync(User user, CancellationToken cancellationToken)
    {
        var userEntity = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = user.Email,
            Password = PasswordHasher.HashPassword(user.Password),
        };
        await _dbContext.Users.AddAsync(userEntity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _dbContext.Entry(userEntity).State = EntityState.Detached;
        return userEntity;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var userEntity = await _dbContext.Users.FindAsync(new object[] { id }, cancellationToken)
            ?? throw new KeyNotFoundException($"User with ID {id} not found.");
        _dbContext.Users.Remove(userEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserEntity> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Users.FindAsync(new object[] { id }, cancellationToken)
            ?? throw new KeyNotFoundException($"User with ID {id} not found.");
    }

    public async Task<List<UserEntity>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        return await _dbContext.Users.AsNoTracking()
            .Where(u => u.Email.Contains(query))
            .ToListAsync(cancellationToken);
    }

    public async Task<UserEntity> UpdateAsync(Guid id, User user, CancellationToken cancellationToken)
    {
        var userEntity = await _dbContext.Users.FindAsync(new object[] { id }, cancellationToken)
            ?? throw new KeyNotFoundException($"User with ID {id} not found.");
        userEntity.Email = user.Email;
        userEntity.Password = PasswordHasher.HashPassword(user.Password);

        _dbContext.Users.Update(userEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _dbContext.Entry(userEntity).State = EntityState.Detached;
        return userEntity;
    }

    public async Task<UserEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<(List<UserEntity> Items, int TotalCount)> ListPaginatedAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbContext.Users.AsNoTracking();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }
}