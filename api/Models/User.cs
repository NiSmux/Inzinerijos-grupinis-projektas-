using Microsoft.AspNetCore.Identity;

namespace MyBackend.Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
    }
}