using System;
using System.Collections;
using System.Reflection;
using Assets.Core.Interfaces;
using OrbCreationExtensions;

namespace Assets.Core.Fucine
{
    public class IdImporter : AbstractImporter
    {
        public IdImporter(CachedFucineProperty cachedFucinePropertyToPopulate, ContentImportLog log) : base(cachedFucinePropertyToPopulate, log)
        {

        }

        public override bool TryImport(AbstractEntity entity, Hashtable entityData, Type entityType)
        {
            if (entity is IEntityWithId entityWithId)
            {
                var idFromData = entityData.GetValue(_cachedFucinePropertyToPopulate.Name);

                if (idFromData!=null)
                    entityWithId.SetId(idFromData as string);
                else 
                    entityWithId.SetId(_cachedFucinePropertyToPopulate.Name); //do I want to make this assumption, if it's a missing id? It's usually right, because we're just using the identifier next level up, but...
                return true;
            }

            else
                Log.LogProblem("ID not specified for a " + entityType.Name);

            return false;
        }
        }
    }
