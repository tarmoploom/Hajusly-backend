using Hajusly.Model;
using Hajusly.Model.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hajusly.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PrivateMessageController : ControllerBase {
    private readonly AppDbContext _context;

    public PrivateMessageController(AppDbContext context) {
        _context = context;
    }


    private Course getCourseByStudentGuid(Guid guid) {

        var s = getStudentByGuid(guid).Id;
        var t = _context.StudentInCourse!.FirstOrDefault(r => r.StudentId == s);
        var course = _context.Courses!.Find(t?.Id);
        if (course == null) {
            throw new InvalidDataException("student not in course");
        }

        return course;

    }



    private Student getStudentByGuid(Guid g) {
        if (_context.Students!.FirstOrDefault(s => s.guid == g) == null) {
            throw new ArgumentException("Student not found");
        }
        return _context.Students!.FirstOrDefault(s => s.guid == g) ?? new Student { };

    }

    /// <summary>
    /// Get private message list for current user
    /// </summary>
    /// <returns>messages, without message text</returns>
    [HttpGet]
    public ActionResult<IEnumerable<PrivateMessageResponse>> GetMessages() {
        var messages = _context.PrivateMessages!.Where(e =>
            e.TeacherId == GetCurrentUserId() && e.isDeleted == false);

        return Ok(messages.Select(m => new PrivateMessageResponse(m, false)));
    }

    /// <summary>
    /// get message by message guid. Includes all 
    /// calling this method marks message as read
    /// </summary>
    /// <param name="id">message guid</param>
    /// <returns>Private Message</returns>
    [HttpGet("{id}")]
    public ActionResult<PrivateMessageResponse> GetMessageById(Guid id) {
        var message = _context.PrivateMessages!.Find(id);
        if (message == null || message.isDeleted || message.TeacherId != GetCurrentUserId()) return NotFound();

        message.Read = DateTime.Now.ToUniversalTime();
        message.isRead = true;
        _context.Entry(message).State = EntityState.Modified;
        _context.SaveChanges();


        return Ok(new PrivateMessageResponse(message, true));
    }

    [HttpPost]
    [AllowAnonymous]
    public IActionResult PostMessage([FromBody] PrivateMessage message, Guid studentGuid) {

        try {
            _context.StudentInCourse!.FirstOrDefault(r => r.guid == studentGuid);
        } catch {
            return NotFound("user not found!");
        }

        // try
        // {
        //     getStudentByGuid(studentGuid);
        //     getCourseByStudentGuid(studentGuid);
        // }
        // catch (Exception e)
        // {
        //     return NotFound("Student not found");
        // }

        var studentinCourse = _context.StudentInCourse!.FirstOrDefault(r => r.guid == studentGuid);

        message.StudentId = studentinCourse!.StudentId; //getStudentByGuid(studentGuid).Id;
        message.courseId = studentinCourse.CourseId; //getCourseByStudentGuid(studentGuid).Id;
        message.TeacherId = _context.Courses!.Find(studentinCourse.CourseId)!.TeacherId; //getCourseByStudentGuid(studentGuid).TeacherId;
        message.Sent = DateTime.Now.ToUniversalTime();
        message.isRead = false;
        _context.Add(message);
        _context.SaveChanges();

        // Email email = new Email();
        // email.message = message.Message;
        // email.subject = message.Subject;
        // email.recipient = _context.Users.Find(GetCurrentUserId()).emailAddress;
        //
        return CreatedAtAction(nameof(GetMessageById), new { Id = message.Id }, new PrivateMessageResponse(message));
    }



    [HttpDelete("{id}")]
    public ActionResult<PrivateMessage> DeleteMessage(Guid id) {
        var message = _context.PrivateMessages!.Find(id);
        if (message == null || message.isDeleted || message.TeacherId != GetCurrentUserId()) return NotFound();

        message.isDeleted = true;
        message.deletedAt = DateTime.Now.ToUniversalTime();
        _context.Entry(message).State = EntityState.Modified;
        _context.SaveChanges();

        return Ok();
    }


    private int GetCurrentUserId() {
        UserHelper currentUser = new UserHelper();
        return currentUser.GetCurrentUserId(HttpContext.User);

    }

    private async void sendEmail(Email message) {
        EmailService sendemail = new EmailService();
        await sendemail.testEmail(message);
    }

}