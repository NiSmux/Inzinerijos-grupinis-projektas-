using Xunit;
using Microsoft.EntityFrameworkCore;
using api.Controllers;
using MyBackend.Data;
using MyBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace MyApp.Tests
{
    public class TasksControllerTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly TasksController _controller;
        private readonly string _mockUserId = "test-user-id";

        public TasksControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);

            _controller = new TasksController(_context);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, _mockUserId)
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetTasks_ReturnsOnlyUserTasks()
        {
            _context.Tasks.AddRange(
                new TaskModel { Title = "Task1", UserId = _mockUserId, Status = "todo" },
                new TaskModel { Title = "Other", UserId = "other-user", Status = "todo" }
            );
            await _context.SaveChangesAsync();

            var result = await _controller.GetTasks();
            var ok = Assert.IsType<OkObjectResult>(result);
            var tasks = Assert.IsType<List<TaskModel>>(ok.Value);
            Assert.Single(tasks);
            Assert.Equal(_mockUserId, tasks[0].UserId);
        }

        [Fact]
        public async Task GetTask_ReturnsTask_WhenExists()
        {
            var task = new TaskModel { Title = "Test", UserId = _mockUserId, Status = "todo" };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            var result = await _controller.GetTask(task.Id);
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<TaskModel>(ok.Value);
            Assert.Equal(task.Id, returned.Id);
        }

        [Fact]
        public async Task GetTask_ReturnsNotFound_WhenNotExists()
        {
            var result = await _controller.GetTask(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateTask_SetsUserId_AndReturnsCreated()
        {
            var task = new TaskModel { Title = "New", Status = "todo" };

            var result = await _controller.CreateTask(task);
            var created = Assert.IsType<CreatedAtActionResult>(result);
            var returned = Assert.IsType<TaskModel>(created.Value);
            Assert.Equal(_mockUserId, returned.UserId);
        }

        [Fact]
        public async Task UpdateTask_ReturnsNoContent_WhenValid()
        {
            var task = new TaskModel { Title = "ToUpdate", UserId = _mockUserId, Status = "todo" };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            task.Title = "Updated";

            var result = await _controller.UpdateTask(task.Id, task);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateTask_ReturnsBadRequest_WhenIdMismatch()
        {
            var task = new TaskModel { Id = 1, Title = "WrongId", Status = "todo" };
            var result = await _controller.UpdateTask(2, task);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Route ID and Task ID do not match.", badRequest.Value);
        }

        [Fact]
        public async Task UpdateTask_ReturnsNotFound_WhenTaskMissing()
        {
            var missing = new TaskModel { Id = 999, Title = "Missing", Status = "todo" };
            var result = await _controller.UpdateTask(999, missing);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteTask_RemovesTask_WhenExists()
        {
            var task = new TaskModel { Title = "ToDelete", UserId = _mockUserId, Status = "todo" };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            var result = await _controller.DeleteTask(task.Id);
            Assert.IsType<NoContentResult>(result);

            var deleted = await _context.Tasks.FindAsync(task.Id);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteTask_ReturnsNotFound_WhenMissing()
        {
            var result = await _controller.DeleteTask(12345);
            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task GetTasks_ReturnsUnauthorized_WhenUserIsMissing()
        {
            var controller = new TasksController(_context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext() // no user set
                }
            };

            var result = await controller.GetTasks();
            Assert.IsType<UnauthorizedResult>(result);
        }
        [Fact]
        public async Task CreateTask_ReturnsUnauthorized_WhenUserIsMissing()
        {
            var controller = new TasksController(_context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext() // no user set
                }
            };

            var newTask = new TaskModel { Title = "NoUser", Status = "todo" };
            var result = await controller.CreateTask(newTask);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("User ID not found from token.", unauthorized.Value);
        }
    }
}
