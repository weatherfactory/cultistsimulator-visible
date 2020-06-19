using System;
using System.Collections;
using System.Reflection;
using Assets.Core.Interfaces;

namespace Assets.Core.Fucine
{
    public class FucineImportDefault : FucineImport
    {
        public FucineImportDefault(PropertyInfo property, ContentImportLogger logger) : base(property, logger)
        {
        }

        public override void Populate(IEntity entity, Hashtable entityData, Type entityType)
        {
            if (Attribute.GetCustomAttribute(_property, typeof(FucineId)) is FucineId )
            {

                _logger.LogProblem("ID not specified for a " + entityType.Name);
            }
            else if (Attribute.GetCustomAttribute(_property, typeof(FucineList)) is FucineList)
            {
                Type listType = _property.PropertyType;
                _property.SetValue(entity, Activator.CreateInstance(listType));
            }
            else if (Attribute.GetCustomAttribute(_property, typeof(FucineDict)) is FucineDict)
            {
                Type dictType = _property.PropertyType;
                _property.SetValue(entity, Activator.CreateInstance(dictType));
            }
            else if (Attribute.GetCustomAttribute(_property, typeof(FucineAspects)) is FucineAspects)
            {
                Type aspectsType = _property.PropertyType;
                _property.SetValue(entity, Activator.CreateInstance(aspectsType));
            }

            else if (Attribute.GetCustomAttribute(_property, typeof(FucineSubEntity)) is FucineSubEntity)
            {
                Type aspectsType = _property.PropertyType;
                _property.SetValue(entity, Activator.CreateInstance(aspectsType));
            }


            else if (Attribute.GetCustomAttribute(_property, typeof(FucineValue)) is FucineValue fucineValueAttr)
            {
                _property.SetValue(entity, fucineValueAttr.DefaultValue);
            }
            else
            {
                _logger.LogProblem($"Unknown or missing Fucine attribute type on {_property.Name} on {entityType.Name}");
            }
        }
    }
}