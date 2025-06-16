using DanskeBank.Communication.Controllers;
using DanskeBank.Communication.Databases;
using DanskeBank.Communication.Databases.Entities;
using DanskeBank.Communication.Models;
using DanskeBank.Communication.Models.Responses;
using DanskeBank.Communication.Repositories;
using DanskeBank.Communication.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DanskeBank.Communication.Tests
{
    public class AuthControllerTests
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

        private static JwtService GetJwtService()
        {
            return new JwtService("supersecretkey1234567890abcdef12345678", "issuer", "audience", 60, 60);
        }

        [Fact]
        public async Task Login_ReturnsOk_WhenCredentialsAreValid()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var password = "password";
            var userEntity = new UserEntity
            {
                Id = Guid.NewGuid(),
                Email = "user@example.com",
                Password = PasswordHasher.HashPassword(password)
            };
            context.Users.Add(userEntity);
            context.SaveChanges();
            var repo = new UserRepository(context);
            var jwtService = GetJwtService();
            var controller = new AuthController(repo, jwtService);
            var user = new User { Email = userEntity.Email, Password = password };
            var result = await controller.Login(user, CancellationToken.None);
            Assert.IsType<OkObjectResult>(result.Result);
            var response = (result.Result as OkObjectResult)?.Value as LoginResponse;
            Assert.True(response?.Success);
            Assert.NotNull(response?.Token);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenUserNotFound()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var repo = new UserRepository(context);
            var jwtService = GetJwtService();
            var controller = new AuthController(repo, jwtService);
            var user = new User { Email = "notfound@example.com", Password = "password" };
            var result = await controller.Login(user, CancellationToken.None);
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var response = (result.Result as UnauthorizedObjectResult)?.Value as LoginResponse;
            Assert.False(response?.Success);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenPasswordInvalid()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var userEntity = new UserEntity
            {
                Id = Guid.NewGuid(),
                Email = "user@example.com",
                Password = PasswordHasher.HashPassword("correctpassword")
            };
            context.Users.Add(userEntity);
            context.SaveChanges();
            var repo = new UserRepository(context);
            var jwtService = GetJwtService();
            var controller = new AuthController(repo, jwtService);
            var user = new User { Email = userEntity.Email, Password = "wrongpassword" };
            var result = await controller.Login(user, CancellationToken.None);
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var response = (result.Result as UnauthorizedObjectResult)?.Value as LoginResponse;
            Assert.False(response?.Success);
        }

        [Fact]
        public void Refresh_ReturnsOk_WhenRefreshTokenValid()
        {
            var jwtService = GetJwtService();
            var (refreshToken, _) = jwtService.GenerateRefreshToken(Guid.NewGuid().ToString(), "user@example.com");
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var repo = new UserRepository(context);
            var controller = new AuthController(repo, jwtService);
            var result = controller.Refresh(refreshToken, CancellationToken.None);
            Assert.IsType<OkObjectResult>(result.Result);
            var response = (result.Result as OkObjectResult)?.Value as LoginResponse;
            Assert.True(response?.Success);
            Assert.NotNull(response?.Token);
        }

        [Fact]
        public void Refresh_ReturnsUnauthorized_WhenRefreshTokenInvalid()
        {
            var jwtService = GetJwtService();
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var repo = new UserRepository(context);
            var controller = new AuthController(repo, jwtService);
            var result = controller.Refresh("invalidtoken", CancellationToken.None);
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var response = (result.Result as UnauthorizedObjectResult)?.Value as LoginResponse;
            Assert.False(response?.Success);
        }
    }
}
