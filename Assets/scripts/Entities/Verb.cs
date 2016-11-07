using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// just a bundle of properties
/// </summary>
    public class Verb
    {
        private string _id;
        private string _label;
        private string _description;

        public string Id
        {
            get { return _id; }
        }

        public string Label
        {
            get { return _label; }
        }

        public string Description
        {
            get { return _description; }
        }

        public Verb(string id, string label, string description)
        {
            _id = id;
            _label = label;
            _description = description;
        }
    }

