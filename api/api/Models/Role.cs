public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } // "Admin", "User", etc.
    public List<User> Users { get; set; }
}
