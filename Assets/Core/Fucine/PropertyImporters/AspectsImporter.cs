using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Core.Fucine.DataImport;
using Assets.Core.Interfaces;
using OrbCreationExtensions;
namespace Assets.Core.Fucine
{
    public class AspectsImporter : AbstractImporter
    {

        public override bool TryImportProperty<T>(T entity, CachedFucineProperty<T> _cachedFucinePropertyToPopulate, EntityData entityData, ContentImportLog log)
        {
            //If no value can be found, initialise the property with a default instance of the correct type, then return
            var aspectsEntity = entityData.ValuesTable[_cachedFucinePropertyToPopulate.LowerCaseName] as EntityData;
            if (aspectsEntity == null)
            {
                _cachedFucinePropertyToPopulate.SetViaFastInvoke(entity, new AspectsDictionary());
                return false;
            }

            IAspectsDictionary aspects = new AspectsDictionary();

            var aspectsAttribute = _cachedFucinePropertyToPopulate.FucineAttribute as FucineAspects;
            var entityProperties =TypeInfoCache<T>.GetCachedFucinePropertiesForType();

            foreach (string k in aspectsEntity.ValuesTable.Keys)
            {
                aspects.Add(k, Convert.ToInt32(aspectsEntity.ValuesTable[k]));
            }

            _cachedFucinePropertyToPopulate.SetViaFastInvoke(entity, aspects);

            if (aspectsAttribute != null)
                ValidateKeysMustExistIn(entity, _cachedFucinePropertyToPopulate, aspectsAttribute.KeyMustExistIn, entityProperties, aspectsEntity.ValuesTable.Keys, log);


            return true;
        }
    }
}