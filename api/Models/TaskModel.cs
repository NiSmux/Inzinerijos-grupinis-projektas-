using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackend.Models
{
    public class TaskModel
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool IsCompleted { get; set; } = false;
        
        [Required]
        public string Status { get; set; } = "todo";

        [ForeignKey("User")]
        public string? UserId { get; set; }

        public User? User { get; set; }
    }
}