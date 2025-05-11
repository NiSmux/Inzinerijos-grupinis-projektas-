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


        
        public string UserID {get; set; }

        
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
