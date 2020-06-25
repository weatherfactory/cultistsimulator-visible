using System;
using System.Collections;
using System.Reflection;
using Assets.Core.Interfaces;
using OrbCreationExtensions;

namespace Assets.Core.Fucine
{
    public class FucineImportId : FucineImport
    {
        public FucineImportId(CachedFucineProperty cachedFucinePropertyToPopulate, ContentImportLog log) : base(cachedFucinePropertyToPopulate, log)
        {

        }

        public override void Populate(AbstractEntity entity, Hashtable entityData, Type entityType)
        {
            if (entity is IEntityWithId entityWithId)
                if(entityData.ContainsKey(_cachedFucinePropertyToPopulate.Name))
                    entityWithId.SetId(entityData.GetValue(_cachedFucinePropertyToPopulate.Name) as string);
                else 
                    entityWithId.SetId(_cachedFucinePropertyToPopulate.Name);

            else
                Log.LogProblem("ID not specified for a " + entityType.Name);
        }
        }
    }
