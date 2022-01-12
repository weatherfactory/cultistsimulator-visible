using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using SecretHistories.Fucine.DataImport;
using SecretHistories.Fucine;

namespace SecretHistories.Fucine
{
    public class ValueImporter : AbstractImporter
    {
        public override bool TryImportProperty<T>(T entity, CachedFucineProperty<T> _cachedFucinePropertyToPopulate, EntityData entityData, ContentImportLog log)
        {
            object valueInData;

            if (_cachedFucinePropertyToPopulate.FucineAttribute.Localise)
                valueInData = entityData.ValuesTable[_cachedFucinePropertyToPopulate.LowerCaseName];
            else
                 valueInData = entityData.ValuesTable[_cachedFucinePropertyToPopulate.LowerCaseName];

            if (valueInData==null)
            {
                _cachedFucinePropertyToPopulate.SetViaFastInvoke(entity,_cachedFucinePropertyToPopulate.FucineAttribute.DefaultValue);
                return false;
            }
            else
            {
                if(_cachedFucinePropertyToPopulate.ThisPropInfo.PropertyType==typeof(float))
                    NoonUtility.Log("aha!");

                try
                {
                    TypeConverter typeConverter = TypeDescriptor.GetConverter(_cachedFucinePropertyToPopulate.ThisPropInfo.PropertyType);
                    string stringRepresentationOfValue = valueInData.ToString();
                    object convertedValue = typeConverter.ConvertFromInvariantString(stringRepresentationOfValue);
                    _cachedFucinePropertyToPopulate.SetViaFastInvoke(entity, convertedValue);
                    return true;
                }
                catch (Exception e)
                {
                    NoonUtility.LogException(e);
                    return false;
                }
    
            }
        }
    }
}