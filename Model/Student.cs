using System.ComponentModel.DataAnnotations.Schema;

namespace Hajusly.Model;

public class Student {
    public int Id { get; set; }
    public string? firstName { get; set; }
    public string? lastName { get; set; }
    public string? studentCode { get; set; }
    public string? email { get; set; }
    public bool isActive { get; set; }
    public Guid guid { get; set; } = getUniqueId();
    public int teacherId { get; set; }

    [NotMapped]
    public int[]? inCourse { get; set; }
    private static Guid getUniqueId() => Guid.NewGuid();


}