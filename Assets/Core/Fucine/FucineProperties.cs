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



    public abstract class Fucine : System.Attribute
    {
        public object DefaultValue { get; protected set; } //might it be necessary to make this dynamic, later?
        public Type ObjectType { get; protected set; }



    }

    public class FucineValue : Fucine
    {

        public FucineValue(int defaultValue)
        {
            ObjectType = typeof(int);
            DefaultValue = defaultValue;
        }

        public FucineValue(string defaultValue)
        {
            DefaultValue = defaultValue;
            ObjectType = typeof(string);

        }

        public FucineValue(bool defaultValue)
        {
            DefaultValue = defaultValue;
            ObjectType = typeof(bool);
        }

        public FucineValue(float defaultValue)
        {
            DefaultValue = defaultValue;
            ObjectType = typeof(float);
        }

        public FucineValue(List<string> l)
        {

        }


    }


    [AttributeUsage(AttributeTargets.Property)]
    public class FucineId : Fucine
    {

        public FucineId()
        {
            ObjectType = typeof(string);
        }

    }


    [AttributeUsage(AttributeTargets.Property)]
    public class FucineList : Fucine
    {
        //public Type MemberType { get; private set; }


        //public FucineList(Type memberType)
        //{
        //    Type listType = typeof(List<>);

        //    Type[] typeArgs = {memberType};

        //    Type constructedType = listType.MakeGenericType(typeArgs);

        //    DefaultValue = Activator.CreateInstance(constructedType);

        //    MemberType = memberType;

        //}

        public FucineList()
        {

        }

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class FucineDictionaryGeneric : Fucine
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
    public class FucineAspectsDictionary : Fucine
    {
        public string KeyMustExistIn { get; set; }
        public FucineAspectsDictionary()
        {
            DefaultValue = new AspectsDictionary();
        }

    }


    [AttributeUsage(AttributeTargets.Property)]
    public class FucineDictStringString : Fucine
    {
        public string KeyMustExistIn { get; set; }

        public FucineDictStringString()
        {
            DefaultValue = new Dictionary<string, string>();
        }

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class FucineDict : Fucine
    {
        public string KeyMustExistIn { get; set; }


    }


    [AttributeUsage(AttributeTargets.Property)]
    public class FucineSubEntity : Fucine
    {
        
        public FucineSubEntity(Type objectType)
        {
            ObjectType = objectType;

            DefaultValue = null;
        }

    }
}
