using System;
using System.Collections;
using System.Reflection;
using Assets.Core.Interfaces;
using OrbCreationExtensions;

namespace Assets.Core.Fucine
{
    public class FucineImportId : FucineImport
    {
        public FucineImportId(PropertyInfo property, ContentImportLogger logger) : base(property, logger)
        {

        }

        public override void Populate(IEntity entity, Hashtable entityData, Type entityType)
        {
            if (entityData.ContainsKey(_property.Name))
                (entity as IEntityUnique).SetId(entityData.GetValue(_property.Name) as string);

            else
            {
                _logger.LogProblem("ID not specified for a " + entityType.Name);
            }
        }
    }
}