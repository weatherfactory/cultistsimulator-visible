using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core
{
    public class ContentImportProblem
    {
        private string _description;

        public string Description
        {
            get { return _description; }
            set { _description = value ?? "Unspecified"; }
        }

        public ContentImportProblem(string description)
        {
            Description = description;
        }
    }
}
