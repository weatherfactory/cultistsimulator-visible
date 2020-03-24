using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core
{
    public class ContentImportMessage
    {
        private string _description;

        public string Description
        {
            get { return _description; }
            set { _description = value ?? "Unspecified"; }
        }

        public int MessageLevel { get; private set; }

        public ContentImportMessage(string description)
        {
            Description = description;
            MessageLevel = 2;
        }

        public ContentImportMessage(string description, int messageLevel)
        {
            Description = description;
            MessageLevel = 0;
        }
    }
}
