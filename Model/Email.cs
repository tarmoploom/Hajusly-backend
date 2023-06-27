namespace Hajusly.Model;

public class Email
{
    public int Id { get; set; }
    public string? subject { get; set; }
    public string? message { get; set; }
    public string? recipient { get; set; }
    
}