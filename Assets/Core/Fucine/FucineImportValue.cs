using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using Assets.Core.Interfaces;

namespace Assets.Core.Fucine
{
    public class FucineImportValue : FucineImport
    {
        public FucineImportValue(CachedFucineProperty cachedFucinePropertyToPopulate, ContentImportLog log) : base(cachedFucinePropertyToPopulate, log)
        {
        }

        public override void Populate(AbstractEntity entity, Hashtable entityData, Type entityType)
        {
            TypeConverter typeConverter = TypeDescriptor.GetConverter(_cachedFucinePropertyToPopulate.PropertyInfo.PropertyType);

            _cachedFucinePropertyToPopulate.PropertyInfo.SetValue(entity, typeConverter.ConvertFromString(entityData[_cachedFucinePropertyToPopulate.Name].ToString()));
        }
    }
}