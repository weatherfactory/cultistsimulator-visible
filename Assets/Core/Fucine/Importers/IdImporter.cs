using System;
using System.Collections;
using System.Reflection;
using Assets.Core.Interfaces;
using OrbCreationExtensions;

namespace Assets.Core.Fucine
{
    public class IdImporter : AbstractImporter
    {
        public override bool TryImport<T>(AbstractEntity<T> entity, CachedFucineProperty<T> _cachedFucinePropertyToPopulate, Hashtable entityData,
            Type entityType, ContentImportLog log)
        {
            if (entity is IEntityWithId entityWithId)
            {
                var idFromData = entityData.GetValue(_cachedFucinePropertyToPopulate.LowerCaseName);

                if (idFromData!=null)
                    entityWithId.SetId(idFromData as string);
                else 
                    entityWithId.SetId(_cachedFucinePropertyToPopulate.LowerCaseName); //do I want to make this assumption, if it's a missing id? It's usually right, because we're just using the identifier next level up, but...
                return true;
            }

            else
                Log.LogProblem("ID not specified for a " + entityType.Name);

            return false;
        }
        }
    }
