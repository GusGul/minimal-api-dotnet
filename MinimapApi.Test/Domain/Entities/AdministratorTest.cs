using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Enumerables;

namespace MinimapApi.Test.Domain.Entities
{
    [TestClass]
    public class AdministratorTest
    {
        [TestMethod]
        public void TestGetSetProperties()
        {
            // Arrange
            var adm = new Administrator
            {
                // Act
                Id = 1,
                Email = "teste@teste.com",
                Password = "password",
                Role = "Admin",
            };

            // Assert
            Assert.AreEqual(1, adm.Id);
            Assert.AreEqual("teste@teste.com", adm.Email);
            Assert.AreEqual("password", adm.Password);
            Assert.AreEqual("Admin", adm.Role);
        }
    }
}