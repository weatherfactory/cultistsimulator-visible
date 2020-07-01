using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Interfaces;

namespace Assets.Core.Fucine
{
   
    public abstract class AbstractImporter
    {
 

        public abstract bool TryImportProperty<T>(T entity, CachedFucineProperty<T> property, Hashtable entityData, ContentImportLog log) where T:AbstractEntity<T>;

        protected void ValidateKeysMustExistIn<T>(T entity, CachedFucineProperty<T> _cachedFucinePropertyToPopulate, string KeyMustExistInPropertyName,
            HashSet<CachedFucineProperty<T>> entityProperties, ICollection keys,ContentImportLog log) where T:AbstractEntity<T>
        {
            if (!string.IsNullOrEmpty(KeyMustExistInPropertyName))
            {
                var mustExistInProperty =
                    entityProperties.SingleOrDefault(p => p.ThisPropInfo.Name == KeyMustExistInPropertyName);
                if (mustExistInProperty != null)
                {
                    foreach (var key in keys)
                    {
                        List<string> acceptableKeys =
                            mustExistInProperty.GetViaFastInvoke(entity) as List<string>;

                        if (acceptableKeys == null)
                            log.LogWarning(
                                $"{entity.GetType().Name} insists that {_cachedFucinePropertyToPopulate.LowerCaseName} should exist in {KeyMustExistInPropertyName}, but that property is empty.");

                        else if (!acceptableKeys.Contains(key))
                            log.LogWarning(
                                $"{entity.GetType().Name} insists that {_cachedFucinePropertyToPopulate.LowerCaseName} should exist in {KeyMustExistInPropertyName}, but the key {key} doesn't.");
                    }
                }
                else
                {
                    log.LogWarning(
                        $"{entity.GetType().Name} insists that {_cachedFucinePropertyToPopulate.LowerCaseName} should exist in {KeyMustExistInPropertyName}, but that property doesn't exist.");
                }
            }
        }
    }
}
