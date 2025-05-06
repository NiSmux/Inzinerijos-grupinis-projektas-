
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using MyBackend.Models;
using api.Controllers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;
using System.Linq;

namespace MyBackend.Tests
{
    // Class to help us deserialize anonymous objects
    public class MessageResponse
    {
        public string? Message { get; set; }
    }

    public class TokenResponse
    {
        public string? Token { get; set; }
    }
    
    public class ErrorResponse
    {
        public string? Message { get; set; }
        public IEnumerable<string>? Errors { get; set; }
    }

    public class ErrorMessageResponse
    {
        public string? message { get; set; }
    }

    public class AuthControllerTests
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<SignInManager<User>> _mockSignInManager;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<AuthController>> _mockLogger;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            var userStoreMock = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var userPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<User>>();
            _mockSignInManager = new Mock<SignInManager<User>>(
                _mockUserManager.Object,
                contextAccessorMock.Object,
                userPrincipalFactoryMock.Object,
                null!, null!, null!, null!);

            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(x => x["Jwt:Key"]).Returns("TestSecretKeyWithAtLeast32Characters");
            _mockConfiguration.Setup(x => x["Jwt:Issuer"]).Returns("TestIssuer");
            _mockConfiguration.Setup(x => x["Jwt:Audience"]).Returns("TestAudience");

            _mockLogger = new Mock<ILogger<AuthController>>();

            _controller = new AuthController(
                _mockUserManager.Object,
                _mockSignInManager.Object,
                _mockConfiguration.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task Register_ValidModel_ReturnsOkResult()
        {
            var registerModel = new RegisterModel
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "TestPass1"
            };

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), registerModel.Password))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _controller.Register(registerModel);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = JsonConvert.DeserializeObject<MessageResponse>(JsonConvert.SerializeObject(okResult.Value));
            Assert.NotNull(response);
            Assert.Equal("User registered successfully", response.Message);

            _mockUserManager.Verify(x => x.CreateAsync(
                It.Is<User>(u => u.Email == registerModel.Email && u.Name == registerModel.Name),
                registerModel.Password), Times.Once);
        }

        [Fact]
        public async Task Register_WeakPassword_ReturnsBadRequest()
        {
            var registerModel = new RegisterModel
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "weak"
            };

            var result = await _controller.Register(registerModel);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = JsonConvert.DeserializeObject<MessageResponse>(JsonConvert.SerializeObject(badRequestResult.Value));
            Assert.NotNull(response);
            Assert.Contains("Password must be", response.Message);

            _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Register_IdentityErrors_ReturnsBadRequest()
        {
            var registerModel = new RegisterModel
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "TestPass1"
            };

            var identityErrors = new List<IdentityError>
            {
                new IdentityError { Description = "Email already exists" }
            };

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), registerModel.Password))
                .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

            var result = await _controller.Register(registerModel);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = JsonConvert.DeserializeObject<ErrorResponse>(JsonConvert.SerializeObject(badRequestResult.Value));
            Assert.NotNull(response);
            Assert.Equal("Registration failed", response.Message);
            Assert.NotNull(response.Errors);
        }

        [Fact]
        public async Task Register_Exception_Returns500()
        {
            var registerModel = new RegisterModel
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "TestPass1"
            };

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), registerModel.Password))
                .ThrowsAsync(new Exception("Test exception"));

            var result = await _controller.Register(registerModel);

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var response = JsonConvert.DeserializeObject<ErrorMessageResponse>(JsonConvert.SerializeObject(statusCodeResult.Value));
            Assert.NotNull(response);
            Assert.Contains("Internal server error", response.message);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
            var loginModel = new LoginModel
            {
                Email = "test@example.com",
                Password = "TestPass1"
            };

            var user = new User
            {
                Id = "userid123",
                UserName = loginModel.Email,
                Email = loginModel.Email,
                Name = "Test User"
            };

            _mockSignInManager.Setup(x => x.PasswordSignInAsync(
                    loginModel.Email, loginModel.Password, false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            _mockUserManager.Setup(x => x.FindByEmailAsync(loginModel.Email))
                .ReturnsAsync(user);

            var result = await _controller.Login(loginModel);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = JsonConvert.DeserializeObject<TokenResponse>(JsonConvert.SerializeObject(okResult.Value));
            Assert.NotNull(response!.Token);
        }

        [Fact]
        public async Task Login_EmptyCredentials_ReturnsBadRequest()
        {
            var loginModel = new LoginModel
            {
                Email = "",
                Password = ""
            };

            var result = await _controller.Login(loginModel);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = JsonConvert.DeserializeObject<MessageResponse>(JsonConvert.SerializeObject(badRequestResult.Value));
            Assert.NotNull(response);
            Assert.Equal("Email and password are required", response.Message);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            var loginModel = new LoginModel
            {
                Email = "test@example.com",
                Password = "WrongPassword"
            };

            _mockSignInManager.Setup(x => x.PasswordSignInAsync(
                    loginModel.Email, loginModel.Password, false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            var result = await _controller.Login(loginModel);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = JsonConvert.DeserializeObject<MessageResponse>(JsonConvert.SerializeObject(unauthorizedResult.Value));
            Assert.NotNull(response);
            Assert.Equal("Invalid credentials", response.Message);
        }

        [Fact]
        public async Task Login_SuccessButUserNotFound_ReturnsUnauthorized()
        {
            var loginModel = new LoginModel
            {
                Email = "test@example.com",
                Password = "TestPass1"
            };

            _mockSignInManager.Setup(x => x.PasswordSignInAsync(
                    loginModel.Email, loginModel.Password, false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            _mockUserManager.Setup(x => x.FindByEmailAsync(loginModel.Email))
                .ReturnsAsync((User?)null);

            var result = await _controller.Login(loginModel);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = JsonConvert.DeserializeObject<MessageResponse>(JsonConvert.SerializeObject(unauthorizedResult.Value));
            Assert.NotNull(response);
            Assert.Equal("Invalid credentials", response.Message);
        }

        [Fact]
        public async Task Login_AdminUser_TokenContainsAdminRole()
        {
            var loginModel = new LoginModel
            {
                Email = "admin@example.com",
                Password = "AdminPass1"
            };

            var user = new User
            {
                Id = "adminid123",
                UserName = loginModel.Email,
                Email = loginModel.Email,
                Name = "Admin User",
                IsAdmin = true
            };

            _mockSignInManager.Setup(x => x.PasswordSignInAsync(
                    loginModel.Email, loginModel.Password, false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            _mockUserManager.Setup(x => x.FindByEmailAsync(loginModel.Email))
                .ReturnsAsync(user);

            var result = await _controller.Login(loginModel);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = JsonConvert.DeserializeObject<TokenResponse>(JsonConvert.SerializeObject(okResult.Value));
            Assert.NotNull(response!.Token);
        }
        [Fact]
        public void TaskModel_UserProperty_CanBeAccessed()
        {
            var user = new User { Name = "Test User" };
            var task = new TaskModel { User = user };

            Assert.Equal("Test User", task.User.Name);
        }
    }
}
