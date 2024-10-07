using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using minimal_api.ModelViews;
using MinimalApi.Domain.DTOs;
using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Enumerables;
using MinimalApi.Domain.Interfaces;
using Moq;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace MinimalApi.Test.Requests
{
    [TestClass]
    public class AdministratorTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly Mock<IAdministratorService> _administratorServiceMock;

        public AdministratorTests(WebApplicationFactory<Program> factory)
        {
            _administratorServiceMock = new Mock<IAdministratorService>();

            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(_administratorServiceMock.Object);
                });
            }).CreateClient();
        }

        [TestMethod]
        public async Task Login_ValidCredentials_ReturnsToken()
        {
            // Arrange
            var loginDTO = new LoginDTO { Email = "teste@teste.com", Password = "password" };
            var administrator = new Administrator { Email = "teste@teste.com", Password = "password", Role = "Admin" };

            _administratorServiceMock.Setup(s => s.Login(loginDTO)).Returns(administrator);

            // Act
            var response = await _client.PostAsJsonAsync("/administrators/login", loginDTO);
            response.EnsureSuccessStatusCode();

            var loggedAdm = await response.Content.ReadFromJsonAsync<LoggedAdm>();

            // Assert
            Assert.IsNotNull(loggedAdm);
            Assert.AreEqual(administrator.Email, loggedAdm.Email);
            Assert.AreEqual(administrator.Role, loggedAdm.Role.ToString());
        }

        [TestMethod]
        public async Task CreateAdministrator_ValidAdministrator_ReturnsCreated()
        {
            // Arrange
            var administratorDTO = new AdministratorDTO
            {
                Email = "newadmin@teste.com",
                Password = "password",
                Role = Role.Admin
            };

            var administrator = new Administrator { Id = 1, Email = administratorDTO.Email, Password = administratorDTO.Password, Role = "Admin" };

            _administratorServiceMock.Setup(s => s.Create(It.IsAny<Administrator>())).Callback(() =>
            {
                administrator.Id = 1;
            });

            // Act
            var response = await _client.PostAsJsonAsync("/administrators", administratorDTO);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            var createdAdm = await response.Content.ReadFromJsonAsync<AdministratorModelView>();
            Assert.AreEqual(administrator.Email, createdAdm.Email);
            Assert.AreEqual(administrator.Role, createdAdm.Role.ToString());
        }
    }
}
