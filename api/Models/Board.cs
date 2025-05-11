// Models/Board.cs
// Multiple To-do-list boards

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TodoListApp.Models
{
    public class Board
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [ForeignKey("UserId")]
        public User? User { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? ModifiedAt { get; set; }
        
        // Navigation property for the tasks in this board
        public ICollection<TodoItem> TodoItems { get; set; } = new List<TodoItem>();
    }
}
