using Hajusly.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hajusly.Controllers;

[Authorize]
[ApiController]
[Route("api/tags")]
public class TagsController : ControllerBase
{
    private readonly AppDbContext m_context;

    public TagsController(AppDbContext context) {
        m_context = context;
    }


    [HttpGet]
    public ActionResult<Tag> GetTags()
    {
        var currentUserId = new UserHelper().GetCurrentUserId(HttpContext.User);
        
        var res = m_context.Tags.Where(it => it.TeacherId == currentUserId);
        return Ok(res);
    }
    
    // anonymous for testing purpose
    [AllowAnonymous]
    [HttpGet("{id}")]
    public ActionResult<Tag> GetTagById([FromRoute] int id)
    {
        var res = m_context.Tags!.FirstOrDefault(it => it.Id == id && it.Trashed == false);
        return Ok(res);
    }
    
    
    [HttpPost]
    public IActionResult PostTag([FromBody] Tag tag) 
    {
        var currentUserId = new UserHelper().GetCurrentUserId(HttpContext.User);
        tag.TeacherId = currentUserId;

        if (m_context.Tags!.AsEnumerable().Any(it => it.CompareTo(tag) == 0))
        {
            return Conflict($"already exists");
        }

        m_context.Tags!.Add(tag);
        m_context.SaveChanges();
        return CreatedAtAction(nameof(GetTagById), new { Id = tag.Id }, tag);
    }
    
    [HttpPost("{tagId:int}/recover")]
    public IActionResult PostTag([FromRoute] int tagId) 
    {
        var currentUserId = new UserHelper().GetCurrentUserId(HttpContext.User);

        var dbItem = m_context.Tags!.FirstOrDefault(it => it.Id == tagId && it.TeacherId == currentUserId);
        if (dbItem == null)
        { return NotFound(); }
        
        dbItem.Trashed = false;
        
        m_context.Tags!.Update(dbItem);
        m_context.SaveChanges();
        
        return Ok();
    }

    
    [HttpPatch]
    public IActionResult PatchTag([FromBody] Tag tag) 
    {
        var currentUserId = new UserHelper().GetCurrentUserId(HttpContext.User);
        tag.TeacherId = currentUserId;

        var dbItem = m_context.Tags!.FirstOrDefault(it => it.Id == tag.Id && it.Trashed == false && it.TeacherId == tag.TeacherId);
        if (dbItem == null)
        { return NotFound(); }
        
        if (m_context.Tags!.AsEnumerable().Any(it => it.CompareTo(tag) == 0))
        {
            return Conflict($"already exists");
        }
        
        dbItem.UpdateFromTag(tag);

        m_context.Tags!.Update(dbItem);
        m_context.SaveChanges();
        
        return AcceptedAtAction(nameof(GetTagById), new { Id = tag.Id }, tag);
    }
    
    [HttpDelete("{tagId:int}")]
    public IActionResult TrashTag([FromRoute] int tagId) 
    {
        var currentUserId = new UserHelper().GetCurrentUserId(HttpContext.User);

        var dbItem = m_context.Tags!.FirstOrDefault(it => it.Id == tagId && it.TeacherId == currentUserId);
        if (dbItem == null)
        { return NotFound(); }
        
        dbItem.Trashed = true;
        
        m_context.Tags!.Update(dbItem);
        m_context.SaveChanges();
        
        return Ok();
    }
    
    [HttpDelete("emptyTrash")]
    public IActionResult TrashTag()
    {
        var currentUserId = new UserHelper().GetCurrentUserId(HttpContext.User);

        var trashedTags = m_context.Tags!.Where(it => it.Trashed == true && it.TeacherId == currentUserId);
        var inBetweenData = m_context.TagInStudentSet!.Where(it => trashedTags.Select(x => x.Id).Contains(it.TagId));
        
        m_context.TagInStudentSet!.RemoveRange(inBetweenData);
        m_context.Tags!.RemoveRange(trashedTags);
        m_context.SaveChanges();
        
        return Ok();
    }
}