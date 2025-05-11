using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoListApp.Models
{
    // User model extending IdentityUser
    public class User : IdentityUser
    {
        public string? Name { get; set; }
        public bool IsAdmin { get; set; } = false;
    }

    // Board model - represents a task board
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

    // TodoItem model - represents a task
    public class TodoItem
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        public bool IsCompleted { get; set; } = false;
        
        [Required]
        public string Status { get; set; } = "todo"; // todo, inprogress, done
        
        public int? Priority { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? ModifiedAt { get; set; }
        
        public DateTime? DueDate { get; set; }
        
        [Required]
        public int BoardId { get; set; }
        
        [ForeignKey("BoardId")]
        public Board? Board { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
