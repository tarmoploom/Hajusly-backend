namespace Hajusly.Model;

public class PrivateMessage
{
    public Guid Id { get; set; }
    public int StudentId { get; set; }
    public int TeacherId { get; set; }
    public int courseId { get; set; }
    public DateTime Sent { get; set; }
    public bool isRead { get; set; } = false;
    public DateTime? Read { get; set; } 
    public string Subject { get; set; }
    public string Message { get; set; }
    public bool isDeleted { get; set; } = false;
    public DateTime? deletedAt { get; set; }
}