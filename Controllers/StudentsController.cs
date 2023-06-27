using System.Security.Claims;
using Hajusly.extensions;
using Hajusly.Model;
using Hajusly.Model.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hajusly.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase {

    private readonly AppDbContext _context;

    public StudentsController(AppDbContext context) {
        _context = context;
    }

    private int GetCurrentUserId() {
        UserHelper currentUser = new UserHelper();
        return currentUser.GetCurrentUserId(HttpContext.User);

    }

    [HttpGet]
    public IActionResult GetStudents() {
        UserHelper currentUser = new UserHelper();
        int currentUserId = currentUser.GetCurrentUserId(HttpContext.User);

        var res = _context.Students!
            .Where(it => it.isActive == true && it.teacherId == currentUserId)
            .ToList();

        return Ok(res.Select(x => x.ToStudentResponse(_context)));

    }

    [HttpGet("{id}")]
    public ActionResult<Student> GetStudentById(int id) {
        var student = _context.Students!.Find(id);

        if (student == null || student.isActive == false || student.teacherId != GetCurrentUserId()) { return NotFound(); }

        return Ok(student.ToStudentResponse(_context));
    }

    //
    // ------ post area ------
    //


    [HttpGet("{id}/sendInvite/{cid}")]
    public async Task<IActionResult> sendCourseInvite(int id, int cid) {
        var student = _context.Students!.Find(id);
        if (student == null || student.isActive == false || student.teacherId != GetCurrentUserId()) return NotFound();

        var invite = _context.StudentInCourse!.FirstOrDefault(t => t.StudentId == id && t.CourseId == cid);

        EmailService sendEmail = new EmailService();
        string subject = "invitation to course";
        string message = $"link to your personal results: https://hajusly.itb2203.tautar.ee/#/myresults/{invite?.guid}";
        await sendEmail.sendEmail(student.email!, subject, message);

        return Ok();
    }


    [HttpPost]
    public IActionResult CreateStudent([FromBody] Student student) {
        var dbStudent = _context.Students!.FirstOrDefault(s => s.Id == student.Id);
        if (dbStudent != null) return Conflict($"Id already exists: {student.Id}");

        student.isActive = true;
        student.guid = getUniqueId();
        student.teacherId = GetCurrentUserId();
        _context.Add(student);
        _context.SaveChanges();

        // try register to courses & if fail return Conflict()
        if (!RegisterToCourses(student)) return Conflict($"Student already registered");

        return CreatedAtAction(nameof(GetStudentById), new { Id = student.Id }, student);
    }
    private bool RegisterToCourses(Student student) {
        if (student.inCourse == null) return false;
        var db = _context.StudentInCourse!.ToList();
        var courses = student.inCourse.ToList();

        foreach (var c in courses) {
            if (db.Any(sc => sc.CourseId == c && sc.StudentId == student.Id)) return false;
        };

        courses.ForEach(c => {
            var sc = new StudentInCourse {
                StudentId = student.Id,
                CourseId = c
            };
            _context.Add(sc);
            _context.SaveChanges();

        });

        return true;
    }

    [HttpPost("{studentId:int}/addTag/{tagId:int}/toCourse/{courseId:int}")]
    public IActionResult AddTagToStudent([FromRoute] int studentId, [FromRoute] int tagId, [FromRoute] int courseId) {
        var student = _context.Students!.Find(studentId);
        if (student == null) { return NotFound(); }

        // no point adding double items
        var dmItem = _context.TagInStudentSet!
            .FirstOrDefault(it => it.TagId == tagId && it.StudentId == studentId && it.CourseId == courseId);
        if (dmItem != null) {
            return BadRequest();
        }

        if (student.teacherId != new UserHelper().GetCurrentUserId(HttpContext.User)) { return BadRequest(); }

        _context.TagInStudentSet!.Add(new TagInStudent {
            StudentId = studentId,
            TagId = tagId,
            CourseId = courseId
        });
        _context.SaveChanges();

        return CreatedAtAction(nameof(GetStudentById), new { Id = student.Id }, student.ToStudentResponse(_context));
    }


    //
    // ------ delete area ------
    //

    [HttpDelete("{id}")]
    public ActionResult<Student> DeleteStudent(int id) {
        var student = _context.Students!.Find(id);
        if (student == null || student.isActive == false || student.teacherId != GetCurrentUserId()) return NotFound();

        student.isActive = false;
        _context.Entry(student).State = EntityState.Modified;
        _context.SaveChanges();

        return Ok();
    }

    [HttpDelete("{studentId:int}/removeTag/{tagId:int}/fromCourse/{courseId:int}")]
    public IActionResult DeleteTagFromStudent([FromRoute] int studentId, [FromRoute] int tagId, [FromRoute] int courseId) {
        var student = _context.Students!.Find(studentId);
        if (student == null || student.isActive == false) { return NotFound(); }

        if (student.teacherId != new UserHelper().GetCurrentUserId(HttpContext.User)) { return BadRequest(); }

        var dmItem = _context.TagInStudentSet!
            .FirstOrDefault(it => it.TagId == tagId && it.StudentId == studentId && it.CourseId == courseId);
        if (dmItem == null) {
            // does not even exists in database, so all guud
            return Ok();
        }

        _context.TagInStudentSet!.Remove(dmItem);
        _context.SaveChanges();

        return Ok();
    }


    private Guid getUniqueId() => Guid.NewGuid();

}
