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

namespace MyApp.Tests
{
    public class TasksControllerTests
    {
        private TasksController GetControllerWithContext(out AppDbContext context, string userId = "test-user-id")
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + System.Guid.NewGuid())
                .Options;

            context = new AppDbContext(options);

            var controller = new TasksController(context);

            // Fake authenticated user
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            return controller;
        }

        [Fact]
        public async Task CreateTask_ShouldReturnCreatedTask()
        {
            var controller = GetControllerWithContext(out var context);

            var newTask = new TaskModel
            {
                Title = "Test Task",
                Description = "Test Desc",
                IsCompleted = false,
                Status = "todo"
            };

            var result = await controller.CreateTask(newTask);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var createdTask = Assert.IsType<TaskModel>(createdResult.Value);

            Assert.Equal("Test Task", createdTask.Title);
        }

        [Fact]
        public async Task GetTasks_ShouldReturnUserTasksOnly()
        {
            var controller = GetControllerWithContext(out var context);

            context.Tasks.AddRange(
                new TaskModel { Title = "User1 Task", UserId = "test-user-id", Status = "todo" },
                new TaskModel { Title = "Other Task", UserId = "someone-else", Status = "todo" }
            );
            await context.SaveChangesAsync();

            var result = await controller.GetTasks();
            var okResult = Assert.IsType<OkObjectResult>(result);
            var tasks = Assert.IsType<List<TaskModel>>(okResult.Value);

            Assert.Single(tasks);
            Assert.Equal("test-user-id", tasks[0].UserId);
        }
    }
}
