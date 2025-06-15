using DanskeBank.Communication.Databases.Entities;
using DanskeBank.Communication.Models;

namespace DanskeBank.Communication.Repositories.Interfaces;

public interface IUserRepository
{
    Task<UserEntity> GetByIdAsync(Guid id);
    Task<UserEntity> AddAsync(User user);
    Task<UserEntity> UpdateAsync(Guid id, User user);
    Task DeleteAsync(Guid id);
    Task<List<UserEntity>> SearchAsync(string query);
    Task<List<UserEntity>> ListAsync();
    Task<UserEntity?> GetByEmailAsync(string email);
}