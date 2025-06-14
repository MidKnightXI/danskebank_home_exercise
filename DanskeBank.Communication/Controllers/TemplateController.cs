using DanskeBank.Communication.Extensions;
using DanskeBank.Communication.Models;
using DanskeBank.Communication.Models.Responses;
using DanskeBank.Communication.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DanskeBank.Communication.Controllers;

[ApiController]
[Route("api/v1/templates")]
public class TemplateController : ControllerBase
{
    private readonly ITemplateRepository _templateRepository;
    private readonly ICustomerRepository _customerRepository;

    public TemplateController(ITemplateRepository templateRepository, ICustomerRepository customerRepository)
    {
        _templateRepository = templateRepository;
        _customerRepository = customerRepository;
    }

    [HttpGet]
    public async Task<ActionResult<TemplatesResponse>> GetTemplates()
    {
        try
        {
            var templates = await _templateRepository.ListAsync();
            return Ok(new TemplatesResponse
            {
                Success = true,
                Templates = templates.ToDtoList()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new TemplatesResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TemplateResponse>> GetTemplate(Guid id)
    {
        try
        {
            var template = await _templateRepository.GetByIdAsync(id);
            return Ok(new TemplateResponse
            {
                Success = true,
                Template = template.ToDto()
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new TemplateResponse
            {
                Success = false,
                Message = $"Template with ID {id} not found."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new TemplateResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpPost]
    public async Task<ActionResult<TemplateResponse>> CreateTemplate([FromBody] Template template)
    {
        try
        {
            var templateEntity = await _templateRepository.AddAsync(template);

            return CreatedAtAction(nameof(GetTemplate), new { id = templateEntity.Id }, new TemplateResponse
            {
                Success = true,
                Template = templateEntity.ToDto()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new TemplateResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TemplateResponse>> UpdateTemplate(Guid id, [FromBody] Template template)
    {
        try
        {
            var templateEntity = await _templateRepository.UpdateAsync(id, template);

            return Ok(new TemplateResponse
            {
                Success = true,
                Template = templateEntity.ToDto()
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new TemplateResponse
            {
                Success = false,
                Message = $"Template with ID {id} not found."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new TemplateResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<BaseResponse>> DeleteTemplate(Guid id)
    {
        try
        {
            await _templateRepository.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new BaseResponse
            {
                Success = false,
                Message = $"Template with ID {id} not found."
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
    public async Task<ActionResult<TemplatesResponse>> SearchTemplates([FromQuery] string query)
    {
        try
        {
            var templates = await _templateRepository.SearchAsync(query);
            return Ok(new TemplatesResponse
            {
                Success = true,
                Templates = templates.ToDtoList()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new TemplatesResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpPost("{templateId}/send/{customerId}")]
    public async Task<ActionResult<BaseResponse>> SendTemplate(Guid templateId, Guid customerId)
    {
        try
        {
            var template = await _templateRepository.GetByIdAsync(templateId);
            var customer = await _customerRepository.GetByIdAsync(customerId);

            if (template is null || customer is null)
            {
                return NotFound(new BaseResponse
                {
                    Success = false,
                    Message = "Template or customer not found."
                });
            }

            template.Body = template.Body
                .Replace("{{CustomerName}}", customer.Name)
                .Replace("{{CustomerEmail}}", customer.Email);

            return Ok(new BaseResponse
            {
                Success = true,
                Message = "Template sent successfully."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new BaseResponse
            {
                Success = false,
                Message = $"Internal server error: {ex.Message}"
            });
        }
    }
}