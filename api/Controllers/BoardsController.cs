// Controllers/BoardsController.cs
// Multiple To-do-list boards

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using MyBackend.Data;
using MyBackend.Models;
using TodoListApp.Models;

namespace TodoListApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BoardsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BoardsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/boards
        [HttpGet]
        public async Task<IActionResult> GetBoards()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var boards = await _context.Boards
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
            
            return Ok(boards);
        }

        // GET: api/boards/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBoard(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var board = await _context.Boards
                .Include(b => b.TodoItems)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
                
            if (board == null)
                return NotFound();

            return Ok(board);
        }

        // POST: api/boards
        [HttpPost]
        public async Task<IActionResult> CreateBoard([FromBody] CreateBoardRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.Title))
                return BadRequest("Board title is required");

            var board = new Board
            {
                Name = request.Title, // Note: Your model uses 'Name' but frontend sends 'title'
                UserId = userId,
                CreatedAt = DateTime.Now
            };
            
            _context.Boards.Add(board);
            await _context.SaveChangesAsync();

            // Return the board in the format expected by frontend
            var response = new
            {
                id = board.Id,
                title = board.Name,
                createdAt = board.CreatedAt,
                userId = board.UserId
            };

            return CreatedAtAction(nameof(GetBoard), new { id = board.Id }, response);
        }

        // PUT: api/boards/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBoard(int id, [FromBody] Board board)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (id != board.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if board exists and belongs to user
            var existingBoard = await _context.Boards.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
            if (existingBoard == null)
                return NotFound();

            // Preserve the original creation date and user ID
            board.CreatedAt = existingBoard.CreatedAt;
            board.UserId = userId;

            _context.Entry(existingBoard).State = EntityState.Detached;
            _context.Entry(board).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BoardExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/boards/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBoard(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var board = await _context.Boards
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
                
            if (board == null)
                return NotFound();

            _context.Boards.Remove(board);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BoardExists(int id)
        {
            return _context.Boards.Any(e => e.Id == id);
        }
    }

    // DTO for create board request
    public class CreateBoardRequest
    {
        public string Title { get; set; } = string.Empty;
    }
}
