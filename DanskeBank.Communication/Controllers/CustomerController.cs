using DanskeBank.Communication.Databases.Entities;
using DanskeBank.Communication.Models;
using Microsoft.AspNetCore.Mvc;

namespace DanskeBank.Communication.Controllers;

[ApiController]
[Route("api/v1/customers")]
public class CustomerController : ControllerBase
{
    [HttpGet]
    public ActionResult<List<CustomerEntity>> GetCustomers()
    {
        return Ok(new List<CustomerEntity>());
    }

    [HttpGet("{id}")]
    public ActionResult<CustomerEntity> GetCustomer(Guid id)
    {
        return Ok(new CustomerEntity { Id = id, Name = "Sample Customer", Email = "customer@example.com" });
    }

    [HttpPost]
    public ActionResult<CustomerEntity> CreateCustomer([FromBody] Customer customer)
    {
        if (customer == null || string.IsNullOrEmpty(customer.Name) || string.IsNullOrEmpty(customer.Email))
        {
            return BadRequest("Invalid customer data.");
        }
        var customerEntity = new CustomerEntity
        {
            Id = Guid.NewGuid(),
            Name = customer.Name,
            Email = customer.Email
        };
        return CreatedAtAction(nameof(GetCustomer), new { id = customerEntity.Id }, customerEntity);
    }

    [HttpPut("{id}")]
    public ActionResult<CustomerEntity> UpdateCustomer(Guid id, [FromBody] Customer customer)
    {
        if (customer == null || string.IsNullOrEmpty(customer.Name) || string.IsNullOrEmpty(customer.Email))
        {
            return BadRequest("Invalid customer data.");
        }

        var customerEntity = new CustomerEntity
        {
            Id = id,
            Name = customer.Name,
            Email = customer.Email
        };
        return Ok(customerEntity);
    }

    [HttpDelete("{id}")]
    public ActionResult DeleteCustomer(Guid id)
    {
        return NoContent();
    }

    [HttpGet("search")]
    public ActionResult<List<CustomerEntity>> SearchCustomers([FromQuery] string query)
    {
        return Ok(new List<CustomerEntity>());
    }
}