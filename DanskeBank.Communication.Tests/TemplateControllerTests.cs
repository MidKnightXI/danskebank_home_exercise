using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DanskeBank.Communication.Controllers;
using DanskeBank.Communication.Databases;
using DanskeBank.Communication.Models;
using DanskeBank.Communication.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DanskeBank.Communication.Tests
{
    public class TemplateControllerTests
    {
        private CommunicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<CommunicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            var context = new CommunicationDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        private ILogger<TemplateController> GetLogger()
        {
            return new LoggerFactory().CreateLogger<TemplateController>();
        }

        [Fact]
        public async Task GetTemplates_ReturnsOkResult()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            context.Templates.Add(new Databases.Entities.TemplateEntity { Id = Guid.NewGuid(), Name = "T1", Subject = "S1", Body = "B1" });
            context.SaveChanges();
            var templateRepo = new TemplateRepository(context);
            var customerRepo = new CustomerRepository(context);
            var logger = GetLogger();
            var controller = new TemplateController(logger, templateRepo, customerRepo);
            var result = await controller.GetTemplates();
            Assert.IsType<OkObjectResult>(result.Result);
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
