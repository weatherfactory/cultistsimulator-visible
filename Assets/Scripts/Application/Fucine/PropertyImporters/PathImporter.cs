using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Fucine;
using SecretHistories.Fucine.DataImport;

namespace SecretHistories.Fucine
{
    public class PathImporter: AbstractImporter
    {
        public override bool TryImportProperty<T>(T entity, CachedFucineProperty<T> _cachedFucinePropertyToPopulate, EntityData entityData, ContentImportLog log)
        {

            object valueInData = entityData.ValuesTable[_cachedFucinePropertyToPopulate.LowerCaseName];

            if (valueInData == null)
            {
                _cachedFucinePropertyToPopulate.SetViaFastInvoke(entity, _cachedFucinePropertyToPopulate.FucineAttribute.DefaultValue);
                return true;
            }
            else
            {

                try
                {
                    string pathValueAsString = valueInData.ToString();
                    FucinePath pathValue = new FucinePath(pathValueAsString);
                    
                    if(pathValue.IsValid())
                    {
                        _cachedFucinePropertyToPopulate.SetViaFastInvoke(entity, pathValue);
                        return true;
                    }
                    else
                    {
                        NoonUtility.Log(
                            $"Tried to import path {pathValueAsString} for an entity of type {nameof(T)}, but there's a problem with it: '{pathValue.IsValid()}'");
                        return false;
                    }
                }
                catch (Exception e)
                {
                    NoonUtility.LogException(e);
                    return false;
                }

                return true;
            }


        }

    }
}
