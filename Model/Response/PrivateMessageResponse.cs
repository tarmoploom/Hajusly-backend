namespace Hajusly.Model.Response;

public class PrivateMessageResponse
{

    public PrivateMessageResponse(PrivateMessage message, bool includeMessage = false)
    {
        this.Id = message.Id;
        this.courseId = message.courseId;
        this.StudentId = message.StudentId;
        this.TeacherId = message.TeacherId;
        this.Subject = message.Subject;
        this.Message = includeMessage ? message.Message : "";
        this.Sent = message.Sent;
        this.isRead = message.isRead;
        
    }
    

    public Guid Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; }

    public int TeacherId { get; set; }
    public int courseId { get; set; }
    public int courseName { get; set; }

    public DateTime Sent { get; set; }
    public bool isRead { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
    
}