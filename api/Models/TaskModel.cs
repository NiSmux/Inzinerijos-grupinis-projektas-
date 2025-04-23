using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}