using Hajusly.Model;
using Hajusly.Model.Response;

namespace Hajusly.extensions;

public static class StudentExtensions {
    public static StudentResponse ToStudentResponse(this Student student, AppDbContext context) {
        return new StudentResponse {
            Id = student.Id,
            firstName = student.firstName,
            lastName = student.lastName,
            studentCode = student.studentCode,
            email = student.email,
            inCourse = context.StudentInCourse!.Where(it => it.StudentId == student.Id).Select(it => it.CourseId).Distinct().ToList()
        };
    }
}