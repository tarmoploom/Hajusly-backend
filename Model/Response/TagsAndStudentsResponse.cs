namespace Hajusly.Model.Response;

public class TagsAndStudentsResponse
{
    public int StudentId { get; set; }
    public List<int> Tags { get; set; } = new List<int>();
}