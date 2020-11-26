using System;
using System.Collections;
using System.Reflection;
using Assets.Core.Fucine.DataImport;
using Assets.Core.Interfaces;
using OrbCreationExtensions;

namespace Assets.Core.Fucine
{
    public class IdImporter : AbstractImporter
    {
        public override bool TryImportProperty<T>(T entity, CachedFucineProperty<T> _cachedFucinePropertyToPopulate, EntityData entityData,ContentImportLog log)
        {
            if (entity is IEntityWithId entityWithId)
            {
                //    var idFromData = entityData.CoreData.GetValue(_cachedFucinePropertyToPopulate.LowerCaseName);
                var idFromData = entityData.Id;

                if (idFromData!=null)
                    entityWithId.SetId(idFromData);
                else 
                    entityWithId.SetId(_cachedFucinePropertyToPopulate.LowerCaseName); //do I want to make this assumption, if it's a missing id? It's usually right, because we're just using the identifier next level up, but...
               //AFAICT the only instance where this currently occurs is an internal deck. It should probably actually use a composite key.
                return true;
            }

            else
                log.LogProblem("ID not specified for a " + typeof(T).Name);

            return false;
        }
        }
    }
