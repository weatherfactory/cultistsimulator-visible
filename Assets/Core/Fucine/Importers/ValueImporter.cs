using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using Assets.Core.Interfaces;

namespace Assets.Core.Fucine
{
    public class ValueImporter : AbstractImporter
    {
        public override bool TryImport<T>(AbstractEntity<T> entity, CachedFucineProperty<T> _cachedFucinePropertyToPopulate, Hashtable entityData, Type entityType, ContentImportLog log)
        {
            object valueInData = entityData[_cachedFucinePropertyToPopulate.LowerCaseName];


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