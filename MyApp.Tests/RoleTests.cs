using Xunit;
using MyBackend.Models;
using System.Collections.Generic;

namespace MyApp.Tests
{
    public class RoleTests
    {
        [Fact]
        public void Role_CanBeCreated_WithProperties()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = "1", Email = "user1@example.com" },
                new User { Id = "2", Email = "user2@example.com" }
            };

            // Act
            var role = new Role
            {
                Id = 1,
                Name = "Admin",
                Users = users
            };

            // Assert
            Assert.Equal(1, role.Id);
            Assert.Equal("Admin", role.Name);
            Assert.Equal(2, role.Users.Count);
            Assert.Equal("user1@example.com", role.Users[0].Email);
        }
    }
}
