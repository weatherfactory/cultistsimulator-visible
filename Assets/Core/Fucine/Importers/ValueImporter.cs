using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using Assets.Core.Interfaces;

namespace Assets.Core.Fucine
{
    public class ValueImporter : AbstractImporter
    {
        public ValueImporter(CachedFucineProperty cachedFucinePropertyToPopulate, ContentImportLog log) : base(cachedFucinePropertyToPopulate, log)
        {
        }

        public override bool TryImport(AbstractEntity entity, Hashtable entityData, Type entityType)
        {
            object valueInData = entityData[_cachedFucinePropertyToPopulate.Name];


            if (valueInData==null)
            {
                _cachedFucinePropertyToPopulate.PropertyInfo.SetValue(entity,
                    _cachedFucinePropertyToPopulate.FucineAttribute.DefaultValue);
                return false;
            }
            else
            {
                TypeConverter typeConverter = TypeDescriptor.GetConverter(_cachedFucinePropertyToPopulate.PropertyInfo.PropertyType);
                _cachedFucinePropertyToPopulate.PropertyInfo.SetValue(entity, typeConverter.ConvertFromString(valueInData.ToString()));
                return true;
            }
        }
    }
}