// Models/Board.cs
// Multiple To-do-list boards

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TodoListApp.Models
{
    public class Board
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation property for the TodoItems associated with this board
        public virtual ICollection<TodoItem> TodoItems { get; set; } = new List<TodoItem>();
    }
}

// Models/TodoItem.cs - Update your existing TodoItem model to include a reference to Board
using System;
using System.ComponentModel.DataAnnotations;

namespace TodoListApp.Models
{
    public class TodoItem
    {
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        public bool IsCompleted { get; set; } = false;
        
        public DateTime DueDate { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Foreign key to Board
        public int BoardId { get; set; }
        
        // Navigation property
        public virtual Board Board { get; set; }
    }
}

// Data/ApplicationDbContext.cs - Update your existing DbContext
using Microsoft.EntityFrameworkCore;
using TodoListApp.Models;

namespace TodoListApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<TodoItem> TodoItems { get; set; }
        public DbSet<Board> Boards { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure the relationship between Board and TodoItem
            modelBuilder.Entity<TodoItem>()
                .HasOne(t => t.Board)
                .WithMany(b => b.TodoItems)
                .HasForeignKey(t => t.BoardId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
