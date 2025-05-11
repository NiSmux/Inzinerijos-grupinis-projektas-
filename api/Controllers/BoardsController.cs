// Controllers/BoardsController.cs
// Multiple To-do-list boards


using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using TodoListApp.Data;
using TodoListApp.Models;

namespace TodoListApp.Controllers
{
    [Authorize]
    public class BoardsController : Controller
    {
        private readonly AppDbContext _context;

        public BoardsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Boards
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

        // GET: Boards/Details/5
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

        // POST: Boards/Create
        [HttpPost]
        public async Task<IActionResult> CreateBoard([FromBody] Board board)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            board.UserId = userId;
            board.CreatedAt = DateTime.Now;
            
            _context.Boards.Add(board);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBoard), new { id = board.Id }, board);
        }

        // PUT: Boards/Edit/5
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

        // DELETE: Boards/Delete/5
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
}
