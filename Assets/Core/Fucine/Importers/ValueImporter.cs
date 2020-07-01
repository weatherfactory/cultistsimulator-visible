using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using Assets.Core.Interfaces;

namespace Assets.Core.Fucine
{
    public class ValueImporter : AbstractImporter
    {
        public override bool TryImport<T>(T entity, CachedFucineProperty<T> _cachedFucinePropertyToPopulate, Hashtable entityData, Type entityType, ContentImportLog log)
        {
            object valueInData = entityData[_cachedFucinePropertyToPopulate.LowerCaseName];


            if (valueInData==null)
            {
                _cachedFucinePropertyToPopulate.SetViaFastInvoke(entity,_cachedFucinePropertyToPopulate.FucineAttribute.DefaultValue);
                return false;
            }
            else
            {
                TypeConverter typeConverter = TypeDescriptor.GetConverter(_cachedFucinePropertyToPopulate.ThisPropInfo.PropertyType);
                _cachedFucinePropertyToPopulate.SetViaFastInvoke(entity, typeConverter.ConvertFromString(valueInData.ToString()));
                return true;
            }
        }
    }
}