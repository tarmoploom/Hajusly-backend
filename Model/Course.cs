using System.ComponentModel.DataAnnotations.Schema;

namespace Hajusly.Model
{
    public class Course
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public DateTime? CourseStart { get; set; }
        public DateTime? CourseEnd { get; set; }
        public int TeacherId { get; set; }
        public bool IsArchived { get; set; } = false;

    }
}