using System.ComponentModel.DataAnnotations;

namespace MyBackend.Models
{
    public class TaskModel
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        [Required]
        public bool IsCompleted { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}