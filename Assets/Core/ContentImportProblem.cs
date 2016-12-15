using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core
{
    public class ContentImportProblem
    {
        public string Description { get; set; }

        public ContentImportProblem(string description)
        {
            Description = description;
        }
    }
}
