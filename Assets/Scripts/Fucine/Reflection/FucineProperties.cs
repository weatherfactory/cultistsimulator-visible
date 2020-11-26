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
        //Note: this doesn't work quite in the way I intended it. Label and Description on Slots are marked as Localise, but
        //the attribute is inspected only for the top level entity. Because the top level entity also has Label and Description marked as localie,
        //the slot properties are added to the localisable keys, but this will break if the names are different. Consider explicitly inspecting subproperty attributes
        //to see if they're also subentities, when loading the data
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
        //anything specified as a FucineSubEntity currently needs a parameterless constructor
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
