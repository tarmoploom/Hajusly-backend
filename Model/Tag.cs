using System.ComponentModel.DataAnnotations.Schema;

namespace Hajusly.Model;

public class Tag : IComparable<Tag>
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public string Name { get; set; }
    public string Color { get; set; }
    public bool Trashed { get; set; } = false;
    
    public int TeacherId { get; set; }

    /// <summary>
    /// if its not part of course then it global tag
    /// </summary>
    public int? PartOfCourseId { get; set; } = null;

    
    
    /// <summary>
    /// check if fields match without Id
    /// </summary>
    /// <param name="other"></param>
    /// <returns> -1 if null, 0 - match, 1 - no match </returns>
    public int CompareTo(Tag? other, int? currentUserId)
    {
        if (other == null)
        { return -1; }

        if (this.Name == other.Name
            && ((currentUserId != null && this.TeacherId == currentUserId) || currentUserId == null)
            && this.TeacherId == other.TeacherId
            && this.PartOfCourseId == other.PartOfCourseId
            // && this.Color == other.Color
            )
        {
            return 0;
        }

        return 1;
    }

    public void UpdateFromTag(Tag tag)
    {
        this.Name = tag.Name;
        this.Color = tag.Color;
        this.PartOfCourseId = tag.PartOfCourseId;
    }

    public int CompareTo(Tag? other)
    {
        return CompareTo(other, null);
    }
}