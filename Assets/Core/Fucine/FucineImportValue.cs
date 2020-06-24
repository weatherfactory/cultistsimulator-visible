using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using Assets.Core.Interfaces;

namespace Assets.Core.Fucine
{
    public class FucineImportValue : FucineImport
    {
        public FucineImportValue(PropertyInfo property, ContentImportLog log) : base(property, log)
        {
        }

        public override void Populate(AbstractEntity entity, Hashtable entityData, Type entityType)
        {
            TypeConverter typeConverter = TypeDescriptor.GetConverter(_property.PropertyType);

            _property.SetValue(entity, typeConverter.ConvertFromString(entityData[_property.Name].ToString()));
        }
    }
}