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


    public abstract class FucineEntityProperty : System.Attribute
    {

    }


    [AttributeUsage(AttributeTargets.Property)]
    public class FucineId : FucineEntityProperty
    {

        public FucineId()
        {
        }

    }


    [AttributeUsage(AttributeTargets.Property)]
    public class FucineInt: FucineEntityProperty
    {
        public int DefaultValue { get;  }

        public FucineInt(int defaultValue)
        {
            DefaultValue = defaultValue;
        }
        
    }

 

    [AttributeUsage(AttributeTargets.Property)]
    public class FucineString : FucineEntityProperty
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
    public class FucineBool : FucineEntityProperty
    {
        public bool DefaultValue { get;  }

        public FucineBool(bool defaultValue)
        {
            DefaultValue = defaultValue;
        }

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class FucineFloat : FucineEntityProperty
    {
        public float DefaultValue { get; }

        public FucineFloat(float defaultValue)
        {
            DefaultValue = defaultValue;
        }

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class FucineListGeneric : FucineEntityProperty
    {
       public dynamic DefaultValue { get; private set; }
        public Type MemberType { get; private set; }


        public FucineListGeneric(Type memberType)
        {
            Type listType = typeof(List<>);

            Type[] typeArgs = {memberType};

            Type constructedType = listType.MakeGenericType(typeArgs);

            DefaultValue = Activator.CreateInstance(constructedType);

            MemberType = memberType;

        }

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class FucineDictionaryGeneric : FucineEntityProperty
    {
        public dynamic DefaultValue { get; private set; }
        public Type KeyType { get; private set; }
        public Type ValueType { get; private set; }


        public FucineDictionaryGeneric(Type keyType,Type valueType)
        {
            Type dictionaryType = typeof(Dictionary<,>);

            Type[] typeArgs = { keyType, valueType };

            Type constructedType = dictionaryType.MakeGenericType(typeArgs);

            DefaultValue = Activator.CreateInstance(constructedType);

            KeyType = keyType;
            ValueType = valueType;

        }

    }


    [AttributeUsage(AttributeTargets.Property)]
    public class FucineAspectsDictionary : FucineEntityProperty
    {
        public IAspectsDictionary DefaultValue { get; }
        public string KeyMustExistIn { get; set; }


        public FucineAspectsDictionary()
        {
            DefaultValue = new AspectsDictionary();
        }

    }


    [AttributeUsage(AttributeTargets.Property)]
    public class FucineDictStringString : FucineEntityProperty
    {
        public Dictionary<string,string> DefaultValue { get; }
        public string KeyMustExistIn { get; set; }


        public FucineDictStringString()
        {
            DefaultValue = new Dictionary<string, string>();
        }

    }


    [AttributeUsage(AttributeTargets.Property)]
    public class FucineEmanationProperty : FucineEntityProperty
    {
        public Type ObjectType { get; }
        public object DefaultValue => null;


        public FucineEmanationProperty(Type objectType)
        {
            ObjectType = objectType;
        }

    }
}
