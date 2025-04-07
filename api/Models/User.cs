using Microsoft.AspNetCore.Identity;

namespace MyBackend.Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
        public bool IsAdmin { get; set; } = false;

        //Role-Based Access Control 
       // public int RoleId { get; set; }
       // public Role Role { get; set; }
    }
}
