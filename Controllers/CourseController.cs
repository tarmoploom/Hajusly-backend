using System.Text;
using Hajusly.extensions;
using Hajusly.Model;
using Hajusly.Model.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Hajusly.Controllers {
    [Authorize]
    [Route("api/courses")]
    [ApiController]
    public class CourseController : ControllerBase {
        private readonly AppDbContext _context;

        public CourseController(AppDbContext context) {
            _context = context;
        }

        //
        // ------ get area ------
        //

        [HttpGet]
        public IActionResult GetCourses() {
            // used from def.h
#if SIMULATE_GET_COURSES_LAG
                        System.Threading.Thread.Sleep(1000);
#endif

            UserHelper currentUser = new UserHelper();
            int currentUserId = currentUser.GetCurrentUserId(HttpContext.User);

            var ret = _context.Courses?.Where(c => c.TeacherId == currentUserId && c.IsArchived == false);
            return Ok(ret);
        }

        [HttpGet("extended")]
        public IActionResult GetCoursesExtended(
            [FromQuery] bool includeStudents = false,
            [FromQuery] bool includeStudentsCount = false,
            [FromQuery] bool includeModules = false,
            [FromQuery] bool includeModulesCount = false,
            [FromQuery] bool onlyArchived = false,
            [FromQuery] bool onlyActive = false
        ) {
            var currentUserId = new UserHelper().GetCurrentUserId(HttpContext.User);

            // var qq =
            //     from course in _context.Courses
            //     join sic in _context.StudentInCourse on course.Id equals sic.CourseId
            //     group new { sic, course } by course.Id into g
            //     select new { Id = g.Key, Data = g.First(it => it.course.Id == g.Key), StudentsCount = g.Count() };
            //
            // return Ok(qq);

            var res = _context.Courses?
                    .Where(c => c.TeacherId == currentUserId)
                    .Where(c => onlyArchived ? c.IsArchived == true : true)
                    .Where(c => onlyActive ? c.IsArchived == false : true)
                // .LeftJoin(
                //     _context.StudentInCourse,
                //     course => course.Id,
                //     sic => sic.CourseId,
                //     (course, sic) => new {
                //         Course = course,
                //         StudentInCourse = sic
                //     }
                // )
                // .LeftJoin(
                //     _context.Students,
                //     x => x.StudentInCourse.StudentId,
                //     y => y.Id,
                //     (x, y) => new {
                //         Course = x.Course,
                //         StudentInCourse = x.StudentInCourse,
                //         Student = y
                //     }
                // )
                // .LeftJoin(
                //     _context.Modules,
                //     x => x.Course.Id,
                //     y => y.CourseId,
                //     (x, y) => new {
                //         Course = x.Course,
                //         StudentInCourse = x.StudentInCourse,
                //         Student = x.Student,
                //         Module = y
                //     }
                // )
                // .GroupBy(
                //     x => x.Course.Id,
                //     y => new { y.Course, y.StudentInCourse, y.Student, y.Module },
                //     (key, g) => new {
                //         // CourseId = key, 
                //         Course = g.First(it => it.Course.Id == key).Course,
                //         Students = includeStudents
                //             ? g.Where(it => it.Course.Id == key)
                //                 .Select(it => it.Student)
                //                 .Where(it => it != null)
                //                 .GroupBy(x => x.Id, y => y, (keyS, gS) => gS.First(x => x.Id == keyS))
                //             : null,
                //         StudentsCount = includeStudentsCount ? g.Select(it => it.Student.Id).Where(it => it != null).Distinct().Count() : (int?)null,
                //         Module = includeModules
                //             ? g.Where(it => it.Course.Id == key)
                //                 .Select(it => it.Module)
                //                 .Where(it => it != null)
                //                 .GroupBy(x => x.Id, y => y, (keyM, gM) => gM.First(x => x.Id == keyM))
                //             : null,
                //         ModulesCount = includeModulesCount ? g.Select(it => it.Module.Id).Where(it => it != null).Distinct().Count() : (int?)null,
                //     }
                // )
                ;

            return Ok(res);
        }

        [HttpGet("{id}")]
        public ActionResult<Course> GetCourse(int id) {
            var course = _context.Courses!.Find(id);

            if (course == null) {
                return NotFound();
            }

            return Ok(course);
        }

        [HttpGet("{id}/modules")]
        public ActionResult<IEnumerable<Module>> GetCourseModules(int id) {
            try {
                // TODO: make it better, looks ugly rn
                Func<int?, List<Module>> getChildren = null;
                getChildren = (parentId) => {
                    return _context.Modules
                        .Where(c => c.ParentModuleId == parentId && c.CourseId == id).ToList()
                        .Select(c => c.SetChildren(getChildren(c.Id).ToList()))
                        .ToList();
                };

                // null bc to get from root
                return Ok(getChildren(null));
            } catch {
                return BadRequest();
            }
        }

        [HttpGet("{courseId}/students")]
        public ActionResult<IEnumerable<StudentResponse>> GetStudents([FromRoute] int courseId) {
            UserHelper currentUser = new UserHelper();
            int currentUserId = currentUser.GetCurrentUserId(HttpContext.User);

            List<Student> res = GetStudentsInCourse(courseId, currentUserId);

            return Ok(res.Select(x => x.ToStudentResponse(_context)));
        }

        private List<Student> GetStudentsInCourse(int courseId, int currentUserId) {
            return _context.Students!.Where(it =>
                    _context.StudentInCourse!
                        .FirstOrDefault(x => x.CourseId == courseId && x.StudentId == it.Id) != null
                        && it.isActive == true && it.teacherId == currentUserId)
                .ToList();
        }

        [HttpGet("{courseId}/points")]
        public ActionResult<IEnumerable<StudentPoints>> LoadCoursePoints([FromRoute] int courseId) {
            var course = _context.Courses!.Find(courseId);
            if (course != null) {
                var modules = _context.Modules!.Where(x => x.CourseId.Equals(courseId));
                List<StudentPoints> points = new List<StudentPoints>();
                foreach (var module in modules) {
                    // if we got parent module then do not count points given,
                    // TODO: those should be removed once sub module has been added
                    if (_context.Modules!.Any(c => c.ParentModuleId == module.Id)) { continue; }

                    var studentPoints = _context.StudentPoints!.Where(x => x.ModuleId.Equals(module.Id) && x.ActiveCredit == true);
                    points.AddRange(studentPoints);
                }
                return Ok(points);
            }
            return NotFound();
        }

        [AllowAnonymous]
        [HttpGet("results/{guid}")]
        public ActionResult<PublicViewResponse> LoadStudentPoints([FromRoute] Guid guid) {
            var studentResult = new PublicViewResponse();

            var studentId = _context.StudentInCourse?.FirstOrDefault(c => c.guid == guid)?.StudentId;
            var courseId = _context.StudentInCourse?.FirstOrDefault(c => c.guid == guid)?.CourseId;

            if (_context.Students!.FirstOrDefault(s => s.Id == studentId) == null) {
                throw new ArgumentException("Student not found");
            }
            var student = _context.Students!.FirstOrDefault(s => s.Id == studentId);

            if (student != null)
                studentResult.StudentHeader = student.firstName + " " + student.lastName + " " + student.studentCode;


            var t = _context.StudentInCourse!.FirstOrDefault(r => r.StudentId == student!.Id);
            var course = _context.Courses!.Find(courseId);
            if (course == null) {
                throw new InvalidDataException("student not in course");
            }

            if (course != null) {
                studentResult.Course = course;
                if (student != null) {
                    List<StudentModuleResult> points = SumAndOrderPoints(course.Id, student);
                    try {
                        Func<int?, List<StudentModuleResult>> getChildren = GetChildrenForModules(points);
                        studentResult.Results = getChildren(null);
                        return Ok(studentResult);
                    } catch {
                        return BadRequest();
                    }
                }
            }
            return NotFound();
        }

        [HttpGet("{courseId}/student/{studentId}")]
        public ActionResult<IEnumerable<StudentModuleResult>> LoadStudentPoints([FromRoute] int courseId, [FromRoute] int studentId) {
            var course = _context.Courses!.Find(courseId);
            if (course != null) {
                var student = _context.Students!.FirstOrDefault(x => x.Id.Equals(studentId));
                if (student != null) {
                    return LoadStudentPoints(courseId, student);
                }
            }
            return NotFound();
        }

        private ActionResult<IEnumerable<StudentModuleResult>> LoadStudentPoints(int courseId, Student? student) {
            List<StudentModuleResult> points = SumAndOrderPoints(courseId, student);

            try {
                Func<int?, List<StudentModuleResult>> getChildren = GetChildrenForModules(points);
                return Ok(getChildren(null));
            } catch {
                return BadRequest();
            }
        }

        private static Func<int?, List<StudentModuleResult>> GetChildrenForModules(List<StudentModuleResult> points) {
            Func<int?, List<StudentModuleResult>> getChildren = null;
            getChildren = (parentId) => {
                return points
                    .Where(c => c.ParentModuleId == parentId).ToList()
                    .Select(c => c.SetChildren(getChildren(c.Id).ToList()))
                    .ToList();
            };
            return getChildren;
        }

        [HttpPost("{courseId}/resultCSV")]
        public FileContentResult ExportCsv([FromRoute] int courseId) {
            var course = _context.Courses!.Find(courseId);
            UserHelper currentUser = new UserHelper();
            int currentUserId = currentUser.GetCurrentUserId(HttpContext.User);

            List<Student> students = GetStudentsInCourse(courseId, currentUserId).OrderBy(x => x.lastName).ToList();

            StringBuilder builder = new StringBuilder();

            // add the first line of the CSV file which consists of the field names
            builder.Append("Code;First name;Last name;Tags");
            List<Module> modules = _context.Modules!.Where(x => x.CourseId.Equals(courseId)).ToList();
            foreach (var module in modules) {
                module.Name = GetFullModuleName(module);
            }
            List<Module> orderedModules = modules!.OrderBy(x => x.Name).ToList();

            foreach (var module in orderedModules) {
                builder.Append(";").Append(module.Name);
            }
            builder.Append(";Course total");
            builder.AppendLine();

            foreach (var row in students) {
                var tags = GetStudentTags(courseId, row, currentUserId);
                List<string> userTags = new List<string>();

                foreach (var id in tags) {
                    var tag = _context.Tags!.FirstOrDefault(x => x.Id.Equals(id));
                    if (tag != null)
                        userTags.Add(tag.Name);
                }
                List<StudentModuleResult> points = SumAndOrderPoints(courseId, row);

                builder.Append(String.Format("{0};{1};{2};{3}", row.studentCode, row.firstName, row.lastName, String.Join(", ", userTags.OrderBy(x => x).ToArray())));
                decimal courseTotal = 0m;

                foreach (var module in orderedModules) {
                    var result = points.Find(x => x.Id.Equals(module.Id));
                    builder.Append(";").Append(result?.ReceivedPoints);
                    if (result?.ParentModuleId == null) {
                        courseTotal = courseTotal + (decimal)result?.ReceivedPoints!;
                    }
                }
                builder.Append(";").Append(courseTotal);
                builder.AppendLine();
            }

            string fileName = "results.csv";

            // return the StringBuilder object as a file specifying the content, the content type and the filename
            return File(new System.Text.UTF8Encoding().GetBytes(builder.ToString()), "text/csv", fileName);
        }

        private string GetFullModuleName(Module module) {
            if (module.ParentModuleId != null) {
                var parent = _context.Modules!.FirstOrDefault(x => x.Id.Equals(module.ParentModuleId));
                module.Name = GetFullModuleName(parent!) + " - " + module.Name;
            }
            return module.Name!;
        }

        private List<StudentModuleResult> SumAndOrderPoints(int courseId, Student? student) {
            List<StudentModuleResult> points = GetCourseModulesForStudent(courseId, student);
            points.OrderBy(x => x.ParentModuleId).ToList();
            foreach (var point in points) {
                addPoints(points, point);
            }

            return points;
        }

        private List<StudentModuleResult> GetCourseModulesForStudent(int courseId, Student? student) {
            var modules = _context.Modules!.Where(x => x.CourseId.Equals(courseId)).OrderBy(x => x.ParentModuleId).ToList();
            List<StudentModuleResult> studentResults = new List<StudentModuleResult>();
            foreach (var module in modules) {
                StudentModuleResult result = new StudentModuleResult();
                result.Id = module.Id;
                result.ModuleName = module.Name;
                result.MaxPoints = module.MaxPoints;
                result.ParentModuleId = module.ParentModuleId;
                var studentPoints = _context.StudentPoints!.FirstOrDefault(x => x.ModuleId.Equals(module.Id) &&
                                                                        x.StudentId.Equals(student!.Id) && x.ActiveCredit == true);
                if (studentPoints != null) {
                    result.ReceivedPoints = studentPoints.Points;
                } else {
                    result.ReceivedPoints = 0;
                }

                studentResults.Add(result);
            }

            return studentResults;
        }

        [HttpGet("{courseId}/tagsAndStudents")]
        public ActionResult<IEnumerable<TagsAndStudentsResponse>> GetTagsAndStudents([FromRoute] int courseId) {
            var currentUserId = new UserHelper().GetCurrentUserId(HttpContext.User);

            var res = _context.Students!.Where(it =>
                _context.StudentInCourse!.FirstOrDefault(x => x.CourseId == courseId && x.StudentId == it.Id) != null
                && it.isActive == true && it.teacherId == currentUserId
            ).ToList();

            return base.Ok(res.Select(x => new TagsAndStudentsResponse {
                StudentId = x.Id,
                Tags = GetStudentTags(courseId, x, currentUserId)

            })
            );
        }

        private List<int> GetStudentTags(int courseId, Student x, int currentUserId) {
            return _context.Tags!.Where(y => y.Trashed == false && y.TeacherId == currentUserId &&
                                                                (
                                                                    _context.TagInStudentSet!.Where(it => it.StudentId == x.Id && it.CourseId == courseId)
                                                                        .Select(it => it.TagId)
                                                                ).Contains(y.Id))
                                    .Select(it => it.Id)
                                    .ToList();
        }

        [HttpGet("{courseId}/tags")]
        public ActionResult<IEnumerable<Tag>> GetTagsByCourse([FromRoute] int courseId) {
            var currentUserId = new UserHelper().GetCurrentUserId(HttpContext.User);

            var res = _context.Tags!.Where(it => (it.PartOfCourseId == null || it.PartOfCourseId == courseId) && it.TeacherId == currentUserId);
            return Ok(res);
        }



        //
        // ------ post area ------
        //

        [HttpPost]
        public ActionResult<Course> PostCourse(Course course) {
            course.CourseStart = (course.CourseStart!.Value).ToUniversalTime();
            course.CourseEnd = (course.CourseEnd!.Value).ToUniversalTime();
            if (course.CourseEnd < course.CourseStart) {
                return BadRequest("End of course has to be after beginning.");
            }

            course.TeacherId = new UserHelper().GetCurrentUserId(HttpContext.User);

            _context.Courses!.Add(course);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, course);
        }

        [HttpPost("createNew")]
        public ActionResult<Course> CreateNewCourse(Course course) {
            var existingCourse = _context.Courses!.FirstOrDefault(x => x.Id.Equals(course.Id));
            if (existingCourse == null)
                return BadRequest("Course not found.");

            Course newCourse = new Course();
            newCourse.CourseStart = (course.CourseStart!.Value).ToUniversalTime();
            newCourse.CourseEnd = (course.CourseEnd!.Value).ToUniversalTime();
            newCourse.Name = course.Name;
            newCourse.Code = course.Code;
            if (course.CourseEnd < course.CourseStart) {
                return BadRequest("End of course has to be after beginning.");
            }

            newCourse.TeacherId = new UserHelper().GetCurrentUserId(HttpContext.User);

            _context.Courses!.Add(newCourse);
            _context.SaveChanges();

            var deadlineDifference = course.CourseEnd - existingCourse.CourseEnd;

            var modules = _context.Modules!.Where(x => x.CourseId.Equals(existingCourse.Id));
            //Add new modules
            foreach (Module module in modules) {
                Module newModule = new Module();
                newModule.Deadline = module.Deadline + deadlineDifference;
                if (newModule.Deadline > course.CourseEnd)
                    newModule.Deadline = course.CourseEnd;
                newModule.OriginalModuleId = module.Id;
                newModule.CourseId = newCourse.Id;
                newModule.Name = module.Name;
                newModule.Abbreviation = module.Abbreviation;
                newModule.MaxPoints = module.MaxPoints;
                newModule.PassingPercent = module.PassingPercent;
                newModule.ParentModuleId = module.ParentModuleId;
                _context.Modules!.Add(newModule);
                _context.SaveChanges();
            }
            //Fix parentModuleId
            var newModules = _context.Modules!.Where(x => x.CourseId.Equals(newCourse.Id));
            foreach (Module module in newModules) {
                if (module.ParentModuleId != null) {
                    var parentModule = newModules.FirstOrDefault(x => x.OriginalModuleId.Equals(module.ParentModuleId));
                    if (parentModule != null) {
                        module.ParentModuleId = parentModule.Id;
                        _context.Entry(module).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        _context.SaveChanges();
                    }
                }
            }

            return CreatedAtAction(nameof(GetCourse), new { id = newCourse.Id }, newCourse);
        }


        [HttpPut("{id}")]
        public IActionResult PutCourse(int id, Course course) {
            if (id != course.Id) {
                return BadRequest("Mismatch in courses");
            }

            var existingCourse = _context.Courses!.FirstOrDefault(x => x.Id.Equals(id));

            if (existingCourse == null) {
                return NotFound("Course not found");
            }
            if (course.CourseEnd < course.CourseStart) {
                return BadRequest("End of course has to be after beginning.");
            }


            existingCourse.Name = course.Name;
            existingCourse.Code = course.Code;
            existingCourse.CourseStart = course.CourseStart;
            existingCourse.CourseEnd = course.CourseEnd;

            _context.Entry(existingCourse).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _context.SaveChanges();

            return Ok(course);
        }

        [HttpPost("{courseId:int}/addStudent")]
        public ActionResult AddStudentToCourse([FromRoute] int courseId, [FromBody] AddStudentToCourseRequest data) {
            var items = _context.StudentInCourse!.ToList();

            // course doesnt exists
            if (_context.Courses!.FirstOrDefault(it => it.Id == courseId) == null) {
                return BadRequest();
            }

            // already exists in course
            if (_context.StudentInCourse!.FirstOrDefault(it =>
                    it.CourseId == courseId && it.StudentId == data.StudentId) != null) {
                return BadRequest();
            }


            var dbItem = new StudentInCourse {
                CourseId = courseId,
                StudentId = data.StudentId
            };

            _context.StudentInCourse!.Add(dbItem);
            _context.SaveChanges();

            return Ok();
        }

        [HttpPost("{courseId:int}/archive")]
        public IActionResult ArchiveCourse([FromRoute] int courseId) {
            var dmItem = _context.Courses!.FirstOrDefault(it => it.Id == courseId);
            if (dmItem == null) { return NotFound(); }

            if (dmItem.TeacherId != new UserHelper().GetCurrentUserId(HttpContext.User)) { return BadRequest(); }

            dmItem.IsArchived = true;

            _context.Courses!.Update(dmItem);
            _context.SaveChanges();
            return Ok();
        }

        [HttpPost("{courseId:int}/unArchive")]
        public IActionResult UnArchiveCourse([FromRoute] int courseId) {
            var dmItem = _context.Courses!.FirstOrDefault(it => it.Id == courseId);
            if (dmItem == null) { return NotFound(); }

            if (dmItem.TeacherId != new UserHelper().GetCurrentUserId(HttpContext.User)) { return BadRequest(); }

            dmItem.IsArchived = false;

            _context.Courses!.Update(dmItem);
            _context.SaveChanges();
            return Ok();
        }



        //
        // ------ delete area ------
        //

        [HttpDelete("{id}")]
        public ActionResult<Course> DeleteCourse(int id) {
            var course = _context.Courses!.Find(id);
            if (course == null) {
                return NotFound();
            }

            _context.Courses!.Remove(course);
            _context.SaveChanges();

            return course;
        }

        [HttpDelete("{courseId:int}/removeStudent/{studentId:int}")]
        public ActionResult RemoveStudentFromCourse([FromRoute] int courseId, [FromRoute] int studentId) {
            var dbItem = _context.StudentInCourse!.FirstOrDefault(it => it.CourseId == courseId && it.StudentId == studentId);
            if (dbItem == null) {
                return NotFound();
            }

            _context.StudentInCourse!.Remove(dbItem);
            _context.SaveChanges();

            return Ok();
        }

        private bool CourseExists(int id) {
            return _context.Courses!.Any(c => c.Id == id);
        }

        private void addPoints(List<StudentModuleResult> results, StudentModuleResult result) {
            if (result.ParentModuleId != null) {
                results.Where(x => x.Id.Equals(result.ParentModuleId)).ToList().ForEach(x => {
                    x.ReceivedPoints += result.ReceivedPoints;
                    addPoints(results, x);
                });
            }
        }



    }
}
