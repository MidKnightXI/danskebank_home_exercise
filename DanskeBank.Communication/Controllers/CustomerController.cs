using DanskeBank.Communication.Extensions;
using DanskeBank.Communication.Models;
using DanskeBank.Communication.Models.Responses;
using DanskeBank.Communication.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DanskeBank.Communication.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/customers")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerController(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    [HttpGet]
    public async Task<ActionResult<CustomersResponse>> GetCustomers()
    {
        try
        {
            var customers = await _customerRepository.ListAsync();
            return Ok(new CustomersResponse
            {
                Success = true,
                Customers = customers.ToDtoList()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new CustomersResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerResponse>> GetCustomer(Guid id)
    {
        try
        {
            var customer = await _customerRepository.GetByIdAsync(id);
          return Ok(new CustomerResponse
            {
                Success = true,
                Customer = customer.ToDto()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new CustomerResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpPost]
    public async Task<ActionResult<CustomerResponse>> CreateCustomer([FromBody] Customer customer)
    {
        if (customer == null || string.IsNullOrEmpty(customer.Name) || string.IsNullOrEmpty(customer.Email))
        {
            return BadRequest(new CustomerResponse
            {
                Success = false,
                Message = "Invalid customer data."
            });
        }
        try
        {
            var customerEntity = await _customerRepository.AddAsync(customer);
            return CreatedAtAction(nameof(GetCustomer), new { id = customerEntity.Id }, new CustomerResponse()
            {
                Success = true,
                Customer = customerEntity.ToDto()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new CustomerResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CustomerResponse>> UpdateCustomer(Guid id, [FromBody] Customer customer)
    {
        try
        {
            var customerEntity = await _customerRepository.UpdateAsync(id, customer);
            return Ok(new CustomerResponse
            {
                Success = true,
                Customer = customerEntity.ToDto()
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new CustomerResponse
            {
                Success = false,
                Message = $"Customer with ID {id} not found."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new CustomerResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<BaseResponse>> DeleteCustomer(Guid id)
    {
        try
        {
            await _customerRepository.GetByIdAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new BaseResponse
            {
                Success = false,
                Message = $"Customer with ID {id} not found."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new BaseResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<CustomersResponse>> SearchCustomers([FromQuery] string query)
    {
        try
        {
            var customers = await _customerRepository.SearchAsync(query);
            return Ok(new CustomersResponse
            {
                Success = true,
                Customers = customers.ToDtoList()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new CustomersResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }
}