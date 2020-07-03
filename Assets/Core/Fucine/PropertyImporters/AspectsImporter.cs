using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Core.Interfaces;
using OrbCreationExtensions;
namespace Assets.Core.Fucine
{
    public class AspectsImporter : AbstractImporter
    {

        public override bool TryImportProperty<T>(T entity, CachedFucineProperty<T> _cachedFucinePropertyToPopulate, Hashtable entityData, ContentImportLog log)
        {
            //If no value can be found, initialise the property with a default instance of the correct type, then return
            var htEntries = entityData.GetHashtable(_cachedFucinePropertyToPopulate.LowerCaseName);
            if (htEntries==null)
            {
                _cachedFucinePropertyToPopulate.SetViaFastInvoke(entity, new AspectsDictionary());
                return false;
            }

            IAspectsDictionary aspects = new AspectsDictionary();

            var aspectsAttribute = _cachedFucinePropertyToPopulate.FucineAttribute as FucineAspects;
            var entityProperties =TypeInfoCache<T>.GetCachedFucinePropertiesForType();

            foreach (string k in htEntries.Keys)
            {
                aspects.Add(k, Convert.ToInt32(htEntries[k]));
            }

            _cachedFucinePropertyToPopulate.SetViaFastInvoke(entity, aspects);

            if (aspectsAttribute != null)
                ValidateKeysMustExistIn(entity, _cachedFucinePropertyToPopulate, aspectsAttribute.KeyMustExistIn, entityProperties, htEntries.Keys, log);


            return true;
        }
    }
}