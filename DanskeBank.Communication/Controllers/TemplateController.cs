using DanskeBank.Communication.Extensions;
using DanskeBank.Communication.Models;
using DanskeBank.Communication.Models.Responses;
using DanskeBank.Communication.Repositories.Interfaces;
using DanskeBank.Communication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DanskeBank.Communication.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/templates")]
public class TemplateController : ControllerBase
{
    private readonly ILogger<TemplateController> _logger;
    private readonly MailingService _mailingService;
    private readonly ITemplateRepository _templateRepository;
    private readonly ICustomerRepository _customerRepository;

    public TemplateController(
        ILogger<TemplateController> logger,
        MailingService mailingService,
        ITemplateRepository templateRepository,
        ICustomerRepository customerRepository
    )
    {
        _logger = logger;
        _mailingService = mailingService;
        _templateRepository = templateRepository;
        _customerRepository = customerRepository;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedTemplatesResponse>> GetTemplates(CancellationToken cancellationToken, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        try
        {
            var (templates, totalCount) = await _templateRepository.ListPaginatedAsync(page, pageSize, cancellationToken);
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            string? next = (page * pageSize < totalCount) ? $"{baseUrl}?page={page + 1}&pageSize={pageSize}" : null;
            string? previous = (page > 1) ? $"{baseUrl}?page={page - 1}&pageSize={pageSize}" : null;
            return Ok(new PaginatedTemplatesResponse
            {
                Success = true,
                Templates = templates.ToDtoList(),
                TotalItems = totalCount,
                Next = next,
                Previous = previous
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new PaginatedTemplatesResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TemplateResponse>> GetTemplate(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var template = await _templateRepository.GetByIdAsync(id, cancellationToken);
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
    public async Task<ActionResult<TemplateResponse>> CreateTemplate([FromBody] Template template, CancellationToken cancellationToken)
    {
        try
        {
            var templateEntity = await _templateRepository.AddAsync(template, cancellationToken);

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
    public async Task<ActionResult<TemplateResponse>> UpdateTemplate(Guid id, [FromBody] Template template, CancellationToken cancellationToken)
    {
        try
        {
            var templateEntity = await _templateRepository.UpdateAsync(id, template, cancellationToken);

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
    public async Task<ActionResult<BaseResponse>> DeleteTemplate(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _templateRepository.DeleteAsync(id, cancellationToken);
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
    public async Task<ActionResult<TemplatesResponse>> SearchTemplates([FromQuery] string query, CancellationToken cancellationToken)
    {
        try
        {
            var templates = await _templateRepository.SearchAsync(query, cancellationToken);
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
    public async Task<ActionResult<BaseResponse>> SendTemplate(Guid templateId, Guid customerId, CancellationToken cancellationToken)
    {
        try
        {
            var template = await _templateRepository.GetByIdAsync(templateId, cancellationToken);
            var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);

            template.Body = _mailingService.FormatEmailBody(template.Body, customer);

            _logger.LogInformation("Sending template '{TemplateName}' to customer '{CustomerName}' ({CustomerEmail}) with body: {TemplateBody}",
                template.Name, customer.Name, customer.Email, template.Body);

            await _mailingService.SendEmailAsync(template.Subject, template.Body, customer.Email, cancellationToken);

            return Ok(new BaseResponse
            {
                Success = true,
                Message = "Template sent successfully."
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new BaseResponse
            {
                Success = false,
                Message = ex.Message
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