using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Core.Fucine


{


    [AttributeUsage(AttributeTargets.Property)]
    public class FucineInt: System.Attribute
    {
        public int DefaultValue { get;  }

        public FucineInt(int defaultValue)
        {
            DefaultValue = defaultValue;
        }
        
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class FucineId : System.Attribute
    {

        public FucineId()
        {
        }

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class FucineString : System.Attribute
    {
        public  string DefaultValue { get;  }

        public bool HasDefaultValue => DefaultValue != null;


        public FucineString()
        {
        }

        public FucineString(string defaultValue)
        {
            DefaultValue = defaultValue;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class FucineBool : System.Attribute
    {
        public bool DefaultValue { get;  }

        public FucineBool(bool defaultValue)
        {
            DefaultValue = defaultValue;
        }

    }
 
    [AttributeUsage(AttributeTargets.Property)]
    public class FucineListString : System.Attribute
    {
        public List<string> DefaultValue { get;  }
        public string MustExistIn { get; set; }

        public FucineListString()
        {
            DefaultValue = new List<string>();
        }

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class FucineAspectsDictionary : System.Attribute
    {
        public IAspectsDictionary DefaultValue { get; }
        public string KeyMustExistIn { get; set; }


        public FucineAspectsDictionary()
        {
            DefaultValue = new AspectsDictionary();
        }

    }


    [AttributeUsage(AttributeTargets.Property)]
    public class FucineDictStringString : System.Attribute
    {
        public Dictionary<string,string> DefaultValue { get; }
        public string KeyMustExistIn { get; set; }


        public FucineDictStringString()
        {
            DefaultValue = new Dictionary<string, string>();
        }

    }
}
