using System.Data.Entity;
using System.Text;
using Hajusly.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hajusly.Controllers;

[Route("api/modules")]
[ApiController]
public class ModuleController : ControllerBase
{
    
    private readonly AppDbContext m_context;

    
    public ModuleController(AppDbContext context)
    {
        m_context = context;
    }
    
    [HttpGet]
    public ActionResult<object> GetModules()
    {
        // TODO: make it better, looks ugly rn
        Func<int?, List<Module>> getChildren = null;
        getChildren = (parentId) => {
            return m_context.Modules
                .Where(c => c.ParentModuleId == parentId).ToList()
                .Select(c => c.SetChildren(getChildren(c.Id).ToList()))
                .ToList();
        };
        
        // null bc to get from root
        return Ok(getChildren(null));
    }

    [HttpGet("{id}")]
    public ActionResult<Module> GetModule(int id) 
    {
        var module = m_context.Modules!.Find(id);

        if (module == null)
        {
            return NotFound();
        }

        return Ok(module);
    }

    [HttpPost]
    public ActionResult<Module> PostModule(Module module)
    {
        if (module.MaxPoints == null) 
            module.MaxPoints = 0;

        var errors = ValidateModule(module);
        if (errors.Length > 0)
            return BadRequest(errors);
        m_context.Modules!.Add(module);
        m_context.SaveChanges();

        return CreatedAtAction(nameof(GetModule), new { id = module.Id }, module);
    }


    [HttpPut("{id}")]
    public IActionResult PutModule(int id, Module module)
    {
        if (id != module.Id)
        {
            return BadRequest("Mismatch in modules");
        }

        var existingModule = m_context.Modules!.FirstOrDefault(x => x.Id.Equals(id));

        if (existingModule == null)
        {
            return NotFound("Module not found");
        }


        var errors = ValidateModule(module);
        if (errors.Length > 0)
            return BadRequest(errors);

        existingModule.Name = module.Name;
        existingModule.Abbreviation = module.Abbreviation;
        existingModule.Deadline = module.Deadline;
        existingModule.MaxPoints =  module.MaxPoints;
        existingModule.PassingPercent = module.PassingPercent;

        m_context.Entry(existingModule).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        m_context.SaveChanges();

        return Ok(module);
    }

    
    [HttpPost("addCredit")]
    public ActionResult<StudentPoints> AddCreditsToStudent(StudentPoints studentPoints)
    {
        var existingCredit = m_context.StudentPoints!.FirstOrDefault(x => x.StudentId.Equals(studentPoints.StudentId) &&
                                                            x.ModuleId.Equals(studentPoints.ModuleId) &&
                                                            x.ActiveCredit.Equals(true));
        if (existingCredit != null)
            existingCredit.ActiveCredit = false;

        studentPoints.Credited = DateTime.Now;
        studentPoints.ActiveCredit = true;
        m_context.StudentPoints!.Add(studentPoints);
        m_context.SaveChanges();

        return CreatedAtAction(nameof(GetModule), new { id = studentPoints.Id }, studentPoints);;
    }

    private string ValidateModule(Module module) {
        StringBuilder builder = new StringBuilder();
        if (module.MaxPoints < 0) 
            builder.AppendLine("Points must be positive.");
        if (module.PassingPercent < 0)
            builder.AppendLine("Percent required must be positive.");
        
        var course = m_context.Courses!.FirstOrDefault(x => x.Id.Equals(module.CourseId));
        if (course == null)
        { 
            builder.AppendLine("Unexpected error, course not found.");
        }
        else
        {
            if (module.Deadline < course.CourseStart)
                builder.AppendLine("Deadline must be after course beginning.");
            if (module.Deadline > course.CourseEnd)
                builder.AppendLine("Deadline must be before course end.");
        }
        return builder.ToString();

    }
}