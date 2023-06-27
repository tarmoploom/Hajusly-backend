using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hajusly.Model.Response
{
    public class StudentModuleResult
    {
        public int Id { get; set; }
        public string? ModuleName { get; set; }
        public int? ParentModuleId { get; set; }
        public decimal? ReceivedPoints { get; set; }
        public decimal? MaxPoints { get; set; }

        public virtual ICollection<StudentModuleResult>? Children { get; set; }

        
        public StudentModuleResult SetChildren(ICollection<StudentModuleResult> children)
        {
            Children = children;
            return this;
        }

    }
}