using DanskeBank.Communication.Databases.Entities;
using DanskeBank.Communication.Models;
using System.Threading;

namespace DanskeBank.Communication.Repositories.Interfaces;

public interface IUserRepository
{
    Task<UserEntity> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<UserEntity> AddAsync(User user, CancellationToken cancellationToken);
    Task<UserEntity> UpdateAsync(Guid id, User user, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<List<UserEntity>> SearchAsync(string query, CancellationToken cancellationToken);
    Task<UserEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<(List<UserEntity> Items, int TotalCount)> ListPaginatedAsync(int page, int pageSize, CancellationToken cancellationToken);
}