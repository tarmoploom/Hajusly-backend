namespace Hajusly.Model.Response;

public class StudentResponse
{
    public int Id { get; set; }
    public string? firstName { get; set; }
    public string? lastName { get; set; }
    public string? studentCode { get; set; }
    public string? email { get; set; }
    public bool isActive { get; set; }
    public List<int> inCourse { get; set; }
    
}