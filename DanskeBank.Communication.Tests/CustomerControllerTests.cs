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
using Xunit;

namespace DanskeBank.Communication.Tests
{
    public class CustomerControllerTests
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

        [Fact]
        public async Task GetCustomers_ReturnsOkResult()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            context.Customers.Add(new Databases.Entities.CustomerEntity { Id = Guid.NewGuid(), Name = "John", Email = "john@example.com" });
            context.SaveChanges();
            var repo = new CustomerRepository(context);
            var controller = new CustomerController(repo);
            var result = await controller.GetCustomers();
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetCustomer_ReturnsOk_WhenFound()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var id = Guid.NewGuid();
            context.Customers.Add(new Databases.Entities.CustomerEntity { Id = id, Name = "John", Email = "john@example.com" });
            context.SaveChanges();
            var repo = new CustomerRepository(context);
            var controller = new CustomerController(repo);
            var result = await controller.GetCustomer(id);
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetCustomer_Returns500_WhenException()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var repo = new CustomerRepository(context);
            var controller = new CustomerController(repo);
            var result = await controller.GetCustomer(Guid.NewGuid());
            Assert.IsType<ObjectResult>(result.Result);
            var objectResult = result.Result as ObjectResult;
            Assert.Equal(500, objectResult?.StatusCode);
        }

        [Fact]
        public async Task CreateCustomer_ReturnsBadRequest_WhenInvalid()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var repo = new CustomerRepository(context);
            var controller = new CustomerController(repo);
            var result = await controller.CreateCustomer(null!);
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateCustomer_ReturnsCreatedAtAction()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var repo = new CustomerRepository(context);
            var controller = new CustomerController(repo);
            var customer = new Customer { Name = "John", Email = "john@example.com" };
            var result = await controller.CreateCustomer(customer);
            Assert.IsType<CreatedAtActionResult>(result.Result);
        }

        [Fact]
        public async Task UpdateCustomer_ReturnsOk_WhenFound()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var id = Guid.NewGuid();
            context.Customers.Add(new Databases.Entities.CustomerEntity { Id = id, Name = "John", Email = "john@example.com" });
            context.SaveChanges();
            var repo = new CustomerRepository(context);
            var controller = new CustomerController(repo);
            var customer = new Customer { Name = "Jane", Email = "jane@example.com" };
            var result = await controller.UpdateCustomer(id, customer);
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateCustomer_ReturnsNotFound_WhenNotFound()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var repo = new CustomerRepository(context);
            var controller = new CustomerController(repo);
            var customer = new Customer { Name = "Jane", Email = "jane@example.com" };
            var result = await controller.UpdateCustomer(Guid.NewGuid(), customer);
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task DeleteCustomer_ReturnsNoContent_WhenFound()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var id = Guid.NewGuid();
            context.Customers.Add(new Databases.Entities.CustomerEntity { Id = id, Name = "John", Email = "john@example.com" });
            context.SaveChanges();
            var repo = new CustomerRepository(context);
            var controller = new CustomerController(repo);
            var result = await controller.DeleteCustomer(id);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteCustomer_ReturnsNotFound_WhenNotFound()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var repo = new CustomerRepository(context);
            var controller = new CustomerController(repo);
            var result = await controller.DeleteCustomer(Guid.NewGuid());
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task SearchCustomers_ReturnsOk()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            context.Customers.Add(new Databases.Entities.CustomerEntity { Id = Guid.NewGuid(), Name = "John", Email = "john@example.com" });
            context.SaveChanges();
            var repo = new CustomerRepository(context);
            var controller = new CustomerController(repo);
            var result = await controller.SearchCustomers("John");
            Assert.IsType<OkObjectResult>(result.Result);
        }
    }
}
