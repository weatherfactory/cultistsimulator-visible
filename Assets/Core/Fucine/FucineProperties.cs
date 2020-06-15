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


    public abstract class FucinePropertyAttribute : System.Attribute
    {
        public object DefaultValue { get; protected set; } //might it be necessary to make this dynamic, later?
        public Type ObjectType { get; protected set; }

    }


    [AttributeUsage(AttributeTargets.Property)]
    public class FucineId : FucinePropertyAttribute
    {

        public FucineId()
        {
            ObjectType = typeof(string);
        }

    }


    [AttributeUsage(AttributeTargets.Property)]
    public class FucineInt: FucinePropertyAttribute
    {

        public FucineInt(int defaultValue)
        {
            ObjectType = typeof(int);
            DefaultValue = defaultValue;
        }
        
    }

 

    [AttributeUsage(AttributeTargets.Property)]
    public class FucineString : FucinePropertyAttribute
    {

        public FucineString()
        {
            DefaultValue = string.Empty;
            ObjectType = typeof(string);
        }

        public FucineString(string defaultValue)
        {
            if (DefaultValue == null)
                DefaultValue = string.Empty;
            else
                DefaultValue = defaultValue;

            ObjectType = typeof(string);

        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class FucineBool : FucinePropertyAttribute
    {

        public FucineBool(bool defaultValue)
        {
            DefaultValue = defaultValue;
            ObjectType = typeof(bool);
        }

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class FucineFloat : FucinePropertyAttribute
    {

        public FucineFloat(float defaultValue)
        {
            DefaultValue = defaultValue;
            ObjectType = typeof(float);
        }

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class FucineListGeneric : FucinePropertyAttribute
    {
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
    public class FucineDictionaryGeneric : FucinePropertyAttribute
    {
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
    public class FucineAspectsDictionary : FucinePropertyAttribute
    {
        public string KeyMustExistIn { get; set; }
        public FucineAspectsDictionary()
        {
            DefaultValue = new AspectsDictionary();
        }

    }


    [AttributeUsage(AttributeTargets.Property)]
    public class FucineDictStringString : FucinePropertyAttribute
    {
        public string KeyMustExistIn { get; set; }

        public FucineDictStringString()
        {
            DefaultValue = new Dictionary<string, string>();
        }

    }


    [AttributeUsage(AttributeTargets.Property)]
    public class FucineEmanationPropertyAttribute : FucinePropertyAttribute
    {
        
        public FucineEmanationPropertyAttribute(Type objectType)
        {
            ObjectType = objectType;

            DefaultValue = null;
        }

    }
}
