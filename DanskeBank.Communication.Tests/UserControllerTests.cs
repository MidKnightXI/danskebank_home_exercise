using DanskeBank.Communication.Controllers;
using DanskeBank.Communication.Databases;
using DanskeBank.Communication.Models;
using DanskeBank.Communication.Repositories;
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

        [Fact]
        public async Task GetUsers_ReturnsOkResult()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            context.Users.Add(new Databases.Entities.UserEntity { Id = Guid.NewGuid(), Email = "user@example.com", Password = "pass" });
            context.SaveChanges();
            var repo = new UserRepository(context);
            var controller = new UserController(repo);
            var result = await controller.GetUsers();
            Assert.IsType<OkObjectResult>(result.Result);
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
            var result = await controller.GetUser(id);
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetUser_ReturnsNotFound_WhenMissing()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var repo = new UserRepository(context);
            var controller = new UserController(repo);
            var result = await controller.GetUser(Guid.NewGuid());
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
            var result = await controller.CreateUser(user);
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
            var result = await controller.UpdateUser(id, user);
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
            var result = await controller.UpdateUser(Guid.NewGuid(), user);
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
            var result = await controller.DeleteUser(id);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteUser_ReturnsNotFound_WhenMissing()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var repo = new UserRepository(context);
            var controller = new UserController(repo);
            var result = await controller.DeleteUser(Guid.NewGuid());
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
