using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hajusly.Model;
using Hajusly.Model.Response;

namespace Hajusly.Model.Response
{
    public class PublicViewResponse
    {
        public string? StudentHeader { get; set; }
        public Course? Course { get; set; }

        public List<StudentModuleResult>? Results { get; set; }
    }
}