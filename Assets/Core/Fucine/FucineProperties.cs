using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Core.Fucine


{

    [AttributeUsage(AttributeTargets.Class)]
    public class FucineImport : System.Attribute
    {
        public string TaggedAs { get; }

        public FucineImport(string taggedAs)
        {
            TaggedAs = taggedAs;
        }
    }


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
            DefaultValue = string.Empty;
        }

        public FucineString(string defaultValue)
        {
            if (DefaultValue == null)
                DefaultValue = string.Empty;
            else
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
    public class FucineFloat : System.Attribute
    {
        public float DefaultValue { get; }

        public FucineFloat(float defaultValue)
        {
            DefaultValue = defaultValue;
        }

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class FucineList : System.Attribute
    {
       public dynamic DefaultValue { get; private set; }
        public Type MemberType { get; private set; }


        public FucineList(Type memberType)
        {
            Type listType = typeof(List<>);

            Type[] typeArgs = {memberType};

            Type constructedType = listType.MakeGenericType(typeArgs);

            DefaultValue = Activator.CreateInstance(constructedType);

            MemberType = memberType;

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


    [AttributeUsage(AttributeTargets.Property)]
    public class FucineEmanationProperty : System.Attribute
    {
        public Type ObjectType { get; }

        public FucineEmanationProperty(Type objectType)
        {
            ObjectType = objectType;
        }

    }
}
