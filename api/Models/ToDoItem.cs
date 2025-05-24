// Models/TodoItem.cs - Update your existing TodoItem model to include a reference to Board
using System;
using System.ComponentModel.DataAnnotations;
using MyBackend.Models;

namespace TodoListApp.Models
{
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
