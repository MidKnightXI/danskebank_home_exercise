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

namespace DanskeBank.Communication.Tests
{
    public class CustomerControllerTests
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

        private static CustomerController CreateController(ICustomerRepository repo)
        {
            var controller = new CustomerController(repo);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = "https";
            httpContext.Request.Host = new HostString("example.com");
            httpContext.Request.Path = "/api/customers";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            return controller;
        }

        [Fact]
        public async Task GetCustomers_DefaultPaging_ReturnsOkWithLinks()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            // seed 25 customers
            for (int i = 1; i <= 25; i++)
            {
                context.Customers.Add(new CustomerEntity { Id = Guid.NewGuid(), Name = $"Name{i}", Email = $"{i}@mail.com" });
            }
            context.SaveChanges();
            var repo = new CustomerRepository(context);
            var controller = CreateController(repo);

            // Act
            var result = await controller.GetCustomers();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<PaginatedCustomersResponse>(ok.Value);
            Assert.True(response.Success);
            Assert.Equal(10, response.Customers?.Count);
            Assert.Equal(25, response.Count);
            Assert.Equal("https://example.com/api/customers?page=2&pageSize=10", response.Next);
            Assert.Null(response.Previous);
        }

        [Fact]
        public async Task GetCustomers_LastPage_NoNextLink()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            // seed 25 customers
            for (int i = 1; i <= 25; i++)
            {
                context.Customers.Add(new CustomerEntity { Id = Guid.NewGuid(), Name = $"Name{i}", Email = $"{i}@mail.com" });
            }
            context.SaveChanges();
            var repo = new CustomerRepository(context);
            var controller = CreateController(repo);

            // Act
            var result = await controller.GetCustomers(page: 3, pageSize: 10);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<PaginatedCustomersResponse>(ok.Value);
            Assert.True(response.Success);
            Assert.Equal(5, response.Customers?.Count);
            Assert.Equal(25, response.Count);
            Assert.Null(response.Next);
            Assert.Equal("https://example.com/api/customers?page=2&pageSize=10", response.Previous);
        }

        [Theory]
        [InlineData(0, 5, 1, 5)]
        [InlineData(5, 0, 5, 10)]
        [InlineData(-1, -1, 1, 10)]
        public async Task GetCustomers_InvalidPageOrSize_ClampsToDefaults(
            int page, int pageSize, int expectedPage, int expectedPageSize)
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            // seed 15 customers for testing
            for (int i = 1; i <= 15; i++)
            {
                context.Customers.Add(new CustomerEntity { Id = Guid.NewGuid(), Name = $"Name{i}", Email = $"{i}@mail.com" });
            }
            context.SaveChanges();
            var repo = new CustomerRepository(context);
            var controller = CreateController(repo);

            // Act
            var result = await controller.GetCustomers(page: page, pageSize: pageSize);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<PaginatedCustomersResponse>(ok.Value);
            Assert.True(response.Success);

            // Calculate expected count
            var skip = (expectedPage - 1) * expectedPageSize;
            var expectedCount = Math.Max(0, Math.Min(15 - skip, expectedPageSize));
            Assert.Equal(expectedCount, response.Customers?.Count);
            Assert.Equal(15, response.Count);

            // Verify next link
            if (expectedPage * expectedPageSize < 15)
                Assert.Equal($"https://example.com/api/customers?page={expectedPage + 1}&pageSize={expectedPageSize}", response.Next);
            else
                Assert.Null(response.Next);

            // Verify previous link
            if (expectedPage > 1)
                Assert.Equal($"https://example.com/api/customers?page={expectedPage - 1}&pageSize={expectedPageSize}", response.Previous);
            else
                Assert.Null(response.Previous);
        }

        [Fact]
        public async Task GetCustomers_RepositoryThrows_ReturnsInternalServerError()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            context.Dispose();
            var repo = new CustomerRepository(context);
            var controller = CreateController(repo);

            // Act
            var actionResult = await controller.GetCustomers();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(500, objectResult.StatusCode);
            var response = Assert.IsType<PaginatedCustomersResponse>(objectResult.Value);
            Assert.False(response.Success);
            Assert.Contains("disposed", response.Message, StringComparison.OrdinalIgnoreCase);
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
            Assert.IsType<NoContentResult>(result.Result);
        }

        [Fact]
        public async Task DeleteCustomer_ReturnsNotFound_WhenNotFound()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var repo = new CustomerRepository(context);
            var controller = new CustomerController(repo);
            var result = await controller.DeleteCustomer(Guid.NewGuid());
            Assert.IsType<NotFoundObjectResult>(result.Result);
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
