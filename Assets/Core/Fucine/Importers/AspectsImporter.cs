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

        public override bool TryImport<T>(T entity, CachedFucineProperty<T> _cachedFucinePropertyToPopulate, Hashtable entityData,
            Type entityType, ContentImportLog log)
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


            if (aspectsAttribute.KeyMustExistIn != null)
            {
                var mustExistInProperty =
                    entityProperties.SingleOrDefault(p => p.LowerCaseName == aspectsAttribute.KeyMustExistIn);
                if (mustExistInProperty != null)
                {
                    foreach (var key in htEntries.Keys)
                    {
                        List<string> acceptableKeys =
                            mustExistInProperty.ThisPropInfo.GetValue(entity) as List<string>;

                        if (acceptableKeys == null)
                            Log.LogProblem(
                                $"{entity.GetType().Name} insists that {_cachedFucinePropertyToPopulate.LowerCaseName} should exist in {mustExistInProperty}, but that property is empty.");

                        if (!acceptableKeys.Contains(key))
                            Log.LogProblem(
                                $"{entity.GetType().Name} insists that {_cachedFucinePropertyToPopulate.LowerCaseName} should exist in {mustExistInProperty}, but the key {key} doesn't.");
                    }
                }
                else
                {
                    Log.LogProblem(
                        $"{entity.GetType().Name} insists that {_cachedFucinePropertyToPopulate.LowerCaseName} should exist in {aspectsAttribute.KeyMustExistIn}, but that property doesn't exist.");
                }
            }

            return true;
        }
    }
}