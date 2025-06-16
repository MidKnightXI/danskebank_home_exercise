using DanskeBank.Communication.Controllers;
using DanskeBank.Communication.Databases;
using DanskeBank.Communication.Databases.Entities;
using DanskeBank.Communication.Models;
using DanskeBank.Communication.Models.Responses;
using DanskeBank.Communication.Repositories;
using DanskeBank.Communication.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DanskeBank.Communication.Tests
{
    public class TemplateControllerTests
    {
        private static CommunicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<CommunicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            var context = new CommunicationDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        private static TemplateController CreateController(ITemplateRepository templateRepo, ICustomerRepository customerRepo)
        {
            var logger = new LoggerFactory().CreateLogger<TemplateController>();
            var controller = new TemplateController(logger, templateRepo, customerRepo);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = "https";
            httpContext.Request.Host = new HostString("example.com");
            httpContext.Request.Path = "/api/templates";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            return controller;
        }

        private ILogger<TemplateController> GetLogger()
        {
            return new LoggerFactory().CreateLogger<TemplateController>();
        }

        [Fact]
        public async Task GetTemplates_DefaultPaging_ReturnsOkWithLinks()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            for (int i = 1; i <= 30; i++)
                context.Templates.Add(new TemplateEntity { Id = Guid.NewGuid(), Name = $"Tpl{i}", Subject = $"Subject{i}", Body = $"Body{i}" });
            context.SaveChanges();

            var templateRepo = new TemplateRepository(context);
            var customerRepo = new CustomerRepository(context);
            var controller = CreateController(templateRepo, customerRepo);
            var result = await controller.GetTemplates();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<PaginatedTemplatesResponse>(ok.Value);
            Assert.True(response.Success);
            Assert.Equal(10, response.Templates?.Count);
            Assert.Equal(30, response.Count);
            Assert.Equal("https://example.com/api/templates?page=2&pageSize=10", response.Next);
            Assert.Null(response.Previous);
        }

        [Fact]
        public async Task GetTemplates_InvalidParams_ClampsAndLinks()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            for (int i = 1; i <= 8; i++)
                context.Templates.Add(new TemplateEntity { Id = Guid.NewGuid(), Name = $"Tpl{i}", Subject = $"Subject{i}", Body = $"Body{i}" });
            context.SaveChanges();

            var templateRepo = new TemplateRepository(context);
            var customerRepo = new CustomerRepository(context);
            var controller = CreateController(templateRepo, customerRepo);
            var result = await controller.GetTemplates(page: 0, pageSize: 20);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<PaginatedTemplatesResponse>(ok.Value);
            Assert.True(response.Success);
            Assert.Equal(8, response.Templates?.Count);
            Assert.Equal(8, response.Count);
            Assert.Null(response.Next);
            Assert.Null(response.Previous);
        }

        [Theory]
        [InlineData(-2, 5, 1, 5)]
        [InlineData(1, -5, 1, 10)]
        public async Task GetTemplates_InvalidPageOrSize_ClampsToDefaults(int page, int pageSize, int expectedPage, int expectedPageSize)
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            for (int i = 1; i <= 12; i++)
                context.Templates.Add(new TemplateEntity { Id = Guid.NewGuid(), Name = $"Tpl{i}", Subject = $"Subject{i}", Body = $"Body{i}" });
            context.SaveChanges();

            var templateRepo = new TemplateRepository(context);
            var customerRepo = new CustomerRepository(context);
            var controller = CreateController(templateRepo, customerRepo);
            var result = await controller.GetTemplates(page: page, pageSize: pageSize);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<PaginatedTemplatesResponse>(ok.Value);
            Assert.True(response.Success);

            var skip = (expectedPage - 1) * expectedPageSize;
            var expectedCount = Math.Max(0, Math.Min(12 - skip, expectedPageSize));
            Assert.Equal(expectedCount, response.Templates?.Count);
            Assert.Equal(12, response.Count);
        }

        [Fact]
        public async Task GetTemplates_RepositoryThrows_ReturnsInternalServerError()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            context.Dispose();
            var templateRepo = new TemplateRepository(context);
            var customerRepo = new CustomerRepository(context);
            var controller = CreateController(templateRepo, customerRepo);

            var actionResult = await controller.GetTemplates();

            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(500, objectResult.StatusCode);
            var response = Assert.IsType<PaginatedTemplatesResponse>(objectResult.Value);
            Assert.False(response.Success);
            Assert.Contains("disposed", response.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetTemplate_ReturnsOk_WhenFound()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var id = Guid.NewGuid();
            context.Templates.Add(new Databases.Entities.TemplateEntity { Id = id, Name = "T1", Subject = "S1", Body = "B1" });
            context.SaveChanges();
            var templateRepo = new TemplateRepository(context);
            var customerRepo = new CustomerRepository(context);
            var logger = GetLogger();
            var controller = new TemplateController(logger, templateRepo, customerRepo);
            var result = await controller.GetTemplate(id);
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetTemplate_ReturnsNotFound_WhenNotFound()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var templateRepo = new TemplateRepository(context);
            var customerRepo = new CustomerRepository(context);
            var logger = GetLogger();
            var controller = new TemplateController(logger, templateRepo, customerRepo);
            var result = await controller.GetTemplate(Guid.NewGuid());
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateTemplate_ReturnsCreatedAtAction()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var templateRepo = new TemplateRepository(context);
            var customerRepo = new CustomerRepository(context);
            var logger = GetLogger();
            var controller = new TemplateController(logger, templateRepo, customerRepo);
            var template = new Template { Name = "T1", Subject = "S1", Body = "B1" };
            var result = await controller.CreateTemplate(template);
            Assert.IsType<CreatedAtActionResult>(result.Result);
        }

        [Fact]
        public async Task UpdateTemplate_ReturnsOk_WhenFound()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var id = Guid.NewGuid();
            context.Templates.Add(new Databases.Entities.TemplateEntity { Id = id, Name = "T1", Subject = "S1", Body = "B1" });
            context.SaveChanges();
            var templateRepo = new TemplateRepository(context);
            var customerRepo = new CustomerRepository(context);
            var logger = GetLogger();
            var controller = new TemplateController(logger, templateRepo, customerRepo);
            var template = new Template { Name = "T2", Subject = "S2", Body = "B2" };
            var result = await controller.UpdateTemplate(id, template);
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateTemplate_ReturnsNotFound_WhenNotFound()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var templateRepo = new TemplateRepository(context);
            var customerRepo = new CustomerRepository(context);
            var logger = GetLogger();
            var controller = new TemplateController(logger, templateRepo, customerRepo);
            var template = new Template { Name = "T2", Subject = "S2", Body = "B2" };
            var result = await controller.UpdateTemplate(Guid.NewGuid(), template);
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task DeleteTemplate_ReturnsNoContent_WhenFound()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var id = Guid.NewGuid();
            context.Templates.Add(new Databases.Entities.TemplateEntity { Id = id, Name = "T1", Subject = "S1", Body = "B1" });
            context.SaveChanges();
            var templateRepo = new TemplateRepository(context);
            var customerRepo = new CustomerRepository(context);
            var logger = GetLogger();
            var controller = new TemplateController(logger, templateRepo, customerRepo);
            var result = await controller.DeleteTemplate(id);
            Assert.IsType<NoContentResult>(result.Result);
        }

        [Fact]
        public async Task DeleteTemplate_ReturnsNotFound_WhenNotFound()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var templateRepo = new TemplateRepository(context);
            var customerRepo = new CustomerRepository(context);
            var logger = GetLogger();
            var controller = new TemplateController(logger, templateRepo, customerRepo);
            var result = await controller.DeleteTemplate(Guid.NewGuid());
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task SearchTemplates_ReturnsOk()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            context.Templates.Add(new Databases.Entities.TemplateEntity { Id = Guid.NewGuid(), Name = "T1", Subject = "S1", Body = "B1" });
            context.SaveChanges();
            var templateRepo = new TemplateRepository(context);
            var customerRepo = new CustomerRepository(context);
            var logger = GetLogger();
            var controller = new TemplateController(logger, templateRepo, customerRepo);
            var result = await controller.SearchTemplates("T1");
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task SendTemplate_ReturnsOk_WhenFound()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var templateId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            context.Templates.Add(new Databases.Entities.TemplateEntity { Id = templateId, Name = "T1", Subject = "S1", Body = "Hello {{CustomerName}}" });
            context.Customers.Add(new Databases.Entities.CustomerEntity { Id = customerId, Name = "John", Email = "john@example.com" });
            context.SaveChanges();
            var templateRepo = new TemplateRepository(context);
            var customerRepo = new CustomerRepository(context);
            var logger = GetLogger();
            var controller = new TemplateController(logger, templateRepo, customerRepo);
            var result = await controller.SendTemplate(templateId, customerId);
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task SendTemplate_ReturnsNotFound_WhenNotFound()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var templateId = Guid.NewGuid();
            context.Templates.Add(new Databases.Entities.TemplateEntity { Id = templateId, Name = "T1", Subject = "S1", Body = "Hello {{CustomerName}}" });
            context.SaveChanges();
            var templateRepo = new TemplateRepository(context);
            var customerRepo = new CustomerRepository(context);
            var logger = GetLogger();
            var controller = new TemplateController(logger, templateRepo, customerRepo);
            var result = await controller.SendTemplate(templateId, Guid.NewGuid());
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
    }
}
