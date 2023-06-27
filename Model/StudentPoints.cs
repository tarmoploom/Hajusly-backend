using System.ComponentModel.DataAnnotations.Schema;

namespace Hajusly.Model;

public class StudentPoints
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public int StudentId { get; set; }
    public int ModuleId { get; set; }
    
    public decimal Points { get; set; }
    public DateTime? Credited { get; set; }
    public bool? ActiveCredit { get; set; }

}