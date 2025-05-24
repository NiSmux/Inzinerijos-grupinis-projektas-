using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using MyBackend.Data; 
using MyBackend.Models;
using Microsoft.AspNetCore.Authorization;

namespace api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TasksController(AppDbContext context)
        {
            _context = context;
        }

        
        
        [HttpGet]
        public async Task<IActionResult> GetTasks(int? boardId)
        {
            var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // If no board specified, get tasks across all user's boards
            if (boardId == null)
            {
                var tasks = await _context.TodoItems
                    .Where(t => t.UserId == userId)
                    .Include(t => t.Board)
                    .ToListAsync();

                return Ok(tasks);
            }
            else
            {
                // Check if board belongs to user
                var board = await _context.Boards
                    .FirstOrDefaultAsync(b => b.Id == boardId && b.UserId == userId);
                
                if (board == null)
                    return NotFound("Board not found or access denied");

                var tasks = await _context.TodoItems
                    .Where(t => t.BoardId == boardId && t.UserId == userId)
                    .ToListAsync();

                return Ok(tasks);
            }
        }
        

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            return Ok(task);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskModel newTask)
        {
            var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found from token.");

            newTask.UserId = userId; // Assign the current user's ID
            _context.Tasks.Add(newTask);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTask), new { id = newTask.Id }, newTask);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskModel updatedTask)
        {
            if (id != updatedTask.Id)
            {
                return BadRequest("Route ID and Task ID do not match.");
            }

            _context.Entry(updatedTask).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Tasks.Any(t => t.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
