using System.ComponentModel.DataAnnotations.Schema;

namespace Hajusly.Model;

[Table("user")]
public class User
{
    [Column("id")]
    public int Id { get; set; }

    [Column("username")]
    public string Username { get; set; } = "";

    [Column("password")]
    public string Password { get; set; } = "";

    [Column("Role")]
    public string? Role { get; set; } = "";

    [Column("email")]
    public string? emailAddress { get; set; } = "";
    
    [Column("DisplayName")]
    public string? name { get; set; } = "";
    
    [Column("isActive")]
    public bool isActive { get; set; } = true;
}