
namespace ThomasPool.Domain.Entities;

public enum Role
{
    User,
    Pending,
    Admin
}

public class AdminInfo
{
    public required string Name { get; set; }
    public required string AdminId { get; set; }
    public required Role Role { get; set; }
}

public class Admin : AdminInfo
{
    public required string Password { get; set; }
}