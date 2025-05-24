using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyBackend.Models;
using TodoListApp.Models;

namespace MyBackend.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TaskModel> Tasks { get; set; }
        public DbSet<Role> Roles { get; set; }

        public DbSet<Board> Boards { get; set; }
        public DbSet<TodoItem> TodoItems { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Role>()
                .Property(r => r.Id)
                .ValueGeneratedNever();

            // Configure the relationship between Board and TodoItem
            modelBuilder.Entity<TodoItem>()
                .HasOne(t => t.Board)
                .WithMany(b => b.TodoItems)
                .HasForeignKey(t => t.BoardId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
