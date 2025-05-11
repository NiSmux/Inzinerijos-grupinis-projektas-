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
