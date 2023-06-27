using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;


namespace Hajusly.Model {
    public class Module {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Abbreviation { get; set; }
        public int? CourseId { get; set; }
        public int? ParentModuleId { get; set; }
        public decimal? MaxPoints { get; set; }
        public int? PassingPercent { get; set; }
        public DateTime? Deadline { get; set; }
        public int? OriginalModuleId { get; set; }



        // public virtual Module ParentModule { get; set; }
        public virtual ICollection<Module>? Children { get; set; }


        public Module SetChildren(ICollection<Module> children) {
            Children = children;
            return this;
        }
    }
}