using System.ComponentModel.DataAnnotations.Schema;

namespace Hajusly.Model;

public class StudentInCourse
{
    
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public Guid guid { get; set; } = Guid.NewGuid();
}