using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using System.Collections.Generic;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("DataPolicy")] // Apply CORS policy here
    public class TasksController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetTasks()
        {
            var tasks = new List<TaskModel>
            {
                new TaskModel { Id = 1, Title = "Pirma užduotis", Description = "Trumpas aprašymas" },
                new TaskModel { Id = 2, Title = "Antra užduotis", Description = "Kitas trumpas aprašymas" }
            };
            return Ok(tasks);
        }
    }

    public class TaskModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
