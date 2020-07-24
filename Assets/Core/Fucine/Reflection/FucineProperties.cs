using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Core.Fucine


{


    [AttributeUsage(AttributeTargets.Class)]
    public class FucineImportable : System.Attribute
    {
        public string TaggedAs { get; }

        public FucineImportable(string taggedAs)
        {
            TaggedAs = taggedAs;
        }
    }


    [AttributeUsage(AttributeTargets.Property)]
    public abstract class Fucine : System.Attribute
    {
        public object DefaultValue { get; set; }

        /// <summary>
        /// This property has no effect, but it's a useful marker we may need at some point
        /// </summary>
        public bool Localise { get; set; }
        public bool ValidateAsElementId { get; set; }
        public Type ObjectType { get; protected set; }
    
        public abstract AbstractImporter CreateImporterInstance();

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class FucineValue : Fucine
    {

        public FucineValue()
        {
        }
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


        public override AbstractImporter CreateImporterInstance()
        {
            return new ValueImporter();
        }
    }


    [AttributeUsage(AttributeTargets.Property)]
    public class FucineId : Fucine
    {

        public FucineId()
        {
            ObjectType = typeof(string);
        }

        public override AbstractImporter CreateImporterInstance()
        {
            return new IdImporter();
        }
    }


    [AttributeUsage(AttributeTargets.Property)]
    public class FucineList : Fucine
    {
        public override AbstractImporter CreateImporterInstance()
        {
            return new ListImporter();

        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class FucineDict : Fucine
    {
        public string KeyMustExistIn { get; set; }
        public override AbstractImporter CreateImporterInstance()
        {
            return new DictImporter();
        }
    }


    [AttributeUsage(AttributeTargets.Property)]
    public class FucineAspects : Fucine
    {
        public string KeyMustExistIn { get; set; }
        public FucineAspects()
        {
            DefaultValue = new AspectsDictionary();
        }

        public override AbstractImporter CreateImporterInstance()
        {
            return new AspectsImporter();

        }
    }
    

    [AttributeUsage(AttributeTargets.Property)]
    public class FucineSubEntity : Fucine
    {
        
        public FucineSubEntity(Type objectType)
        {
            ObjectType = objectType;

            DefaultValue = null;
        }

        public override AbstractImporter CreateImporterInstance()
        {
            return new SubEntityImporter();

        }
    }
}
