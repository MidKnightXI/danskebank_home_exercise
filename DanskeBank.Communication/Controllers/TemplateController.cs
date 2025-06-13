using DanskeBank.Communication.Databases.Entities;
using DanskeBank.Communication.Models;
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
    public ActionResult<List<TemplateEntity>> GetTemplates()
    {
        return Ok(new List<TemplateEntity>());
    }

    [HttpGet("{id}")]
    public ActionResult<TemplateEntity> GetTemplate(Guid id)
    {
        return Ok(new TemplateEntity { Id = id, Name = "Sample Template", Subject = "Template Subject", Body = "Template content" });
    }

    [HttpPost]
    public ActionResult<TemplateEntity> CreateTemplate([FromBody] Template template)
    {
        if (template == null || string.IsNullOrEmpty(template.Name) || string.IsNullOrEmpty(template.Body))
        {
            return BadRequest("Invalid template data.");
        }
        var templateEntity = new TemplateEntity
        {
            Id = Guid.NewGuid(),
            Name = template.Name,
            Subject = template.Subject,
            Body = template.Body
        };
        return CreatedAtAction(nameof(GetTemplate), new { id = templateEntity.Id }, templateEntity);
    }

    [HttpPut("{id}")]
    public ActionResult<TemplateEntity> UpdateTemplate(Guid id, [FromBody] Template template)
    {
        if (template == null || string.IsNullOrEmpty(template.Name) || string.IsNullOrEmpty(template.Body))
        {
            return BadRequest("Invalid template data.");
        }

        var templateEntity = new TemplateEntity
        {
            Id = id,
            Name = template.Name,
            Subject = template.Subject,
            Body = template.Body
        };
        return Ok(templateEntity);
    }

    [HttpDelete("{id}")]
    public ActionResult DeleteTemplate(Guid id)
    {
        return NoContent();
    }

    [HttpGet("search")]
    public ActionResult<List<TemplateEntity>> SearchTemplates([FromQuery] string query)
    {
        return Ok(new List<TemplateEntity>());
    }

    [HttpPost("{id}/send")]
    public ActionResult SendTemplate(Guid id, [FromBody] Customer customer)
    {
        if (customer == null || string.IsNullOrEmpty(customer.Name) || string.IsNullOrEmpty(customer.Email))
        {
            return BadRequest("Invalid customer data.");
        }

        return Ok($"Template {id} sent to {customer.Name} at {customer.Email}.");
    }
}