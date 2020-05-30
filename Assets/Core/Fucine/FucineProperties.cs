using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Core.Fucine
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FucineIntProperty: System.Attribute
    {
        private readonly int _defaultValue;

        public FucineIntProperty(int defaultValue)
        {
            _defaultValue = defaultValue;
        }
        
    }


    [AttributeUsage(AttributeTargets.Property)]
    public class FucineStringProperty : System.Attribute
    {
        public  string DefaultValue { get; set; }

        public FucineStringProperty(string defaultValue)
        {
            DefaultValue = defaultValue;
        }

    }
}
