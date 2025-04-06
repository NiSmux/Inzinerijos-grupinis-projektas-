using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyBackend.Models;

namespace MyBackend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSet for user entities (assumed to be already defined)
        public DbSet<User> Users { get; set; }

        // DbSet for task entities
        public DbSet<TaskModel> Tasks { get; set; }

        //Role-Based Access Control (RBAC)
        public DbSet<Role> Roles { get; set; }
        
    }
}
