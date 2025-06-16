using DanskeBank.Communication.Databases.Entities;
using DanskeBank.Communication.Models;

namespace DanskeBank.Communication.Repositories.Interfaces;

public interface ICustomerRepository
{
    Task<CustomerEntity> GetByIdAsync(Guid id);
    Task<CustomerEntity> AddAsync(Customer customer);
    Task<CustomerEntity> UpdateAsync(Guid id, Customer customer);
    Task DeleteAsync(Guid id);
    Task<List<CustomerEntity>> SearchAsync(string query);
    Task<(List<CustomerEntity> Items, int TotalCount)> ListPaginatedAsync(int page, int pageSize);
}