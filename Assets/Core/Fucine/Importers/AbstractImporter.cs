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


        public abstract bool TryImportProperty<T>(T entity, CachedFucineProperty<T> propertyToValidate,
            Hashtable entityData, ContentImportLog log) where T : AbstractEntity<T>;



        protected void ValidateKeysMustExistIn<T>(T entity, CachedFucineProperty<T> _cachedFucinePropertyToPopulate,
            string KeyMustExistInPropertyName,
            HashSet<CachedFucineProperty<T>> entityProperties, ICollection keysToValidate, ContentImportLog log)
            where T : AbstractEntity<T>
        {
            var keysToValidateList = keysToValidate.Cast<string>().ToList();


            if (!string.IsNullOrEmpty(KeyMustExistInPropertyName))
            {
                var propertyToValidateAgainst =
                    entityProperties.SingleOrDefault(p => p.ThisPropInfo.Name == KeyMustExistInPropertyName);

                if (propertyToValidateAgainst == null)
                    log.LogWarning(
                        $"{entity.GetType().Name} insists that {_cachedFucinePropertyToPopulate.LowerCaseName} should exist in {KeyMustExistInPropertyName}, but that property doesn't exist.");
                else
                {
                    List<string> validKeys =
                        propertyToValidateAgainst.GetViaFastInvoke(entity) as List<string>;

                    if (validKeys == null)
                        log.LogWarning(
                            $"{entity.GetType().Name} insists that {_cachedFucinePropertyToPopulate.LowerCaseName} should exist in {KeyMustExistInPropertyName}, but that property is empty.");
                    else
                    {
                        var missingKeys = keysToValidateList.Except(validKeys);

                        foreach (var missingKey in missingKeys)
                        {
                            log.LogWarning(
                                $"{entity.GetType().Name} insists that {_cachedFucinePropertyToPopulate.LowerCaseName} should exist in {KeyMustExistInPropertyName}, but the key {missingKey} doesn't.");
                        }

                    }
                }
            }
        }

    }
}
