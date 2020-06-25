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



    public abstract class Fucine : System.Attribute
    {
        public object DefaultValue { get; protected set; } //might it be necessary to make this dynamic, later?
        public Type ObjectType { get; protected set; }

        public abstract AbstractFucineImporter CreateImporterInstance(CachedFucineProperty property,ContentImportLog log);

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


        public override AbstractFucineImporter CreateImporterInstance(CachedFucineProperty property, ContentImportLog log)
        {
            return new ValueImporter(property,log);
        }
    }


    [AttributeUsage(AttributeTargets.Property)]
    public class FucineId : Fucine
    {

        public FucineId()
        {
            ObjectType = typeof(string);
        }

        public override AbstractFucineImporter CreateImporterInstance(CachedFucineProperty property, ContentImportLog log)
        {
            return new IdImporter(property, log);
        }
    }


    [AttributeUsage(AttributeTargets.Property)]
    public class FucineList : Fucine
    {
        public override AbstractFucineImporter CreateImporterInstance(CachedFucineProperty property, ContentImportLog log)
        {
            return new ListImporter(property, log);

        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class FucineDict : Fucine
    {
        public string KeyMustExistIn { get; set; }


        public override AbstractFucineImporter CreateImporterInstance(CachedFucineProperty property, ContentImportLog log)
        {
            return new DictImporter(property, log);
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

        public override AbstractFucineImporter CreateImporterInstance(CachedFucineProperty property, ContentImportLog log)
        {
            return new AspectsImporter(property, log);

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

        public override AbstractFucineImporter CreateImporterInstance(CachedFucineProperty property, ContentImportLog log)
        {
            return new SubEntityImporter(property, log);

        }
    }
}
