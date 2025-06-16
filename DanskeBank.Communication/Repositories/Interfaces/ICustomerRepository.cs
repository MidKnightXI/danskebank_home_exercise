using DanskeBank.Communication.Databases.Entities;
using DanskeBank.Communication.Models;
using System.Threading;

namespace DanskeBank.Communication.Repositories.Interfaces;

public interface ICustomerRepository
{
    Task<CustomerEntity> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<CustomerEntity> AddAsync(Customer customer, CancellationToken cancellationToken);
    Task<CustomerEntity> UpdateAsync(Guid id, Customer customer, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<List<CustomerEntity>> SearchAsync(string query, CancellationToken cancellationToken);
    Task<(List<CustomerEntity> Items, int TotalCount)> ListPaginatedAsync(int page, int pageSize, CancellationToken cancellationToken);
}