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
    public class UserControllerTests
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

        /// <summary>
        /// Creates a new UserController with a setted up HttpContext for the paginated response.<br/>
        /// This is necessary to generate the correct links in the response.
        /// </summary>
        /// <param name="repo"></param>
        private static UserController CreateController(IUserRepository repo)
        {
            var controller = new UserController(repo);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = "https";
            httpContext.Request.Host = new HostString("example.com");
            httpContext.Request.Path = "/api/users";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            return controller;
        }

        [Fact]
        public async Task GetUsers_Pagination_WorksCorrectly()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            for (int i = 1; i <= 12; i++)
                context.Users.Add(new UserEntity { Id = Guid.NewGuid(), Email = $"User{i}@example.com", Password = "pass" });
            context.SaveChanges();

            var repo = new UserRepository(context);
            var controller = CreateController(repo);
            var result = await controller.GetUsers(CancellationToken.None, page: 2, pageSize: 5);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<PaginatedUsersResponse>(ok.Value);
            Assert.True(response.Success);
            Assert.Equal(5, response.Users?.Count);
            Assert.Equal(12, response.TotalItems);
            Assert.Equal("https://example.com/api/users?page=3&pageSize=5", response.Next);
            Assert.Equal("https://example.com/api/users?page=1&pageSize=5", response.Previous);
        }

        [Theory]
        [InlineData(0, 3, 1, 3)]
        [InlineData(2, 0, 2, 10)]
        public async Task GetUsers_InvalidPageOrSize_ClampsToDefaults(int page, int pageSize, int expectedPage, int expectedPageSize)
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            for (int i = 1; i <= 9; i++)
                context.Users.Add(new UserEntity { Id = Guid.NewGuid(), Email = $"User{i}@example.com", Password = "pass" });
            context.SaveChanges();

            var repo = new UserRepository(context);
            var controller = CreateController(repo);
            var result = await controller.GetUsers(CancellationToken.None, page: page, pageSize: pageSize);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<PaginatedUsersResponse>(ok.Value);
            Assert.True(response.Success);

            var skip = (expectedPage - 1) * expectedPageSize;
            var expectedCount = Math.Max(0, Math.Min(9 - skip, expectedPageSize));
            Assert.Equal(expectedCount, response.Users?.Count);
            Assert.Equal(9, response.TotalItems);
        }

        [Fact]
        public async Task GetUsers_RepositoryThrows_ReturnsInternalServerError()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            context.Dispose();
            var repo = new UserRepository(context);
            var controller = CreateController(repo);

            var actionResult = await controller.GetUsers(CancellationToken.None);

            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(500, objectResult.StatusCode);
            var response = Assert.IsType<PaginatedUsersResponse>(objectResult.Value);
            Assert.False(response.Success);
            Assert.Contains("disposed", response.Message, StringComparison.OrdinalIgnoreCase);
        }


        [Fact]
        public async Task GetUser_ReturnsOk_WhenFound()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var id = Guid.NewGuid();
            context.Users.Add(new Databases.Entities.UserEntity { Id = id, Email = "user@example.com", Password = "pass" });
            context.SaveChanges();
            var repo = new UserRepository(context);
            var controller = new UserController(repo);
            var result = await controller.GetUser(id, CancellationToken.None);
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetUser_ReturnsNotFound_WhenMissing()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var repo = new UserRepository(context);
            var controller = new UserController(repo);
            var result = await controller.GetUser(Guid.NewGuid(), CancellationToken.None);
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateUser_ReturnsCreatedAtAction()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var repo = new UserRepository(context);
            var controller = new UserController(repo);
            var user = new User { Email = "newuser@example.com", Password = "pass" };
            var result = await controller.CreateUser(user, CancellationToken.None);
            Assert.IsType<CreatedAtActionResult>(result.Result);
        }

        [Fact]
        public async Task UpdateUser_ReturnsOk_WhenFound()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var id = Guid.NewGuid();
            context.Users.Add(new Databases.Entities.UserEntity { Id = id, Email = "user@example.com", Password = "pass" });
            context.SaveChanges();
            var repo = new UserRepository(context);
            var controller = new UserController(repo);
            var user = new User { Email = "updated@example.com", Password = "newpass" };
            var result = await controller.UpdateUser(id, user, CancellationToken.None);
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateUser_ReturnsNotFound_WhenMissing()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var repo = new UserRepository(context);
            var controller = new UserController(repo);
            var user = new User { Email = "updated@example.com", Password = "newpass" };
            var result = await controller.UpdateUser(Guid.NewGuid(), user, CancellationToken.None);
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task DeleteUser_ReturnsNoContent_WhenFound()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var id = Guid.NewGuid();
            context.Users.Add(new Databases.Entities.UserEntity { Id = id, Email = "user@example.com", Password = "pass" });
            context.SaveChanges();
            var repo = new UserRepository(context);
            var controller = new UserController(repo);
            var result = await controller.DeleteUser(id, CancellationToken.None);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteUser_ReturnsNotFound_WhenMissing()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var repo = new UserRepository(context);
            var controller = new UserController(repo);
            var result = await controller.DeleteUser(Guid.NewGuid(), CancellationToken.None);
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
