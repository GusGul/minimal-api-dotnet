using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MinimalApi.Domain.Entities;
using MinimalApi.Infraestructure.Db;

namespace MinimalApi.Test.Services
{
    [TestClass]
    public class AdministratorService
    {
        [TestMethod]
        private Context CreateTestContext()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            return new Context(options);
        }

        [TestMethod]
        public void TestCreateAdministrator()
        {
            // Arrange
            var adm = new Administrator
            {
                Id = 1,
                Email = "teste@teste.com",
                Password = "password",
                Role = "Admin",
            };

            // Act
            using (var context = CreateTestContext())
            {
                context.Administrators.Add(adm);
                context.SaveChanges();

                var createAdm = context.Administrators.FirstOrDefault(a => a.Id == 1);
                // Assert
                Assert.IsNotNull(createAdm);
                Assert.AreEqual(1, createAdm.Id);
                Assert.AreEqual("teste@teste.com", createAdm.Email);
                Assert.AreEqual("password", createAdm.Password);
                Assert.AreEqual("Admin", createAdm.Role);
            }
        }
    }
}
