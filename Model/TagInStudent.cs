using System.ComponentModel.DataAnnotations.Schema;

namespace Hajusly.Model;

public class TagInStudent
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public int TagId { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }

}