using System;
using System.Collections;
using System.Reflection;
using Assets.Core.Interfaces;
using OrbCreationExtensions;

namespace Assets.Core.Fucine
{
    public class FucineImportId : FucineImport
    {
        public FucineImportId(PropertyInfo property, ContentImportLog log) : base(property, log)
        {

        }

        public override void Populate(AbstractEntity entity, Hashtable entityData, Type entityType)
        {
            if (entity is IEntityWithId entityWithId)
                if(entityData.ContainsKey(_property.Name))
                    entityWithId.SetId(entityData.GetValue(_property.Name) as string);
                else 
                    entityWithId.SetId(_property.Name);

            else
                Log.LogProblem("ID not specified for a " + entityType.Name);
        }
        }
    }
