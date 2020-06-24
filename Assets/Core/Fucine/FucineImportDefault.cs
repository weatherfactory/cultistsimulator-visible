using System;
using System.Collections;
using System.Reflection;
using Assets.Core.Interfaces;

namespace Assets.Core.Fucine
{
    public class FucineImportDefault : FucineImport
    {
        public FucineImportDefault(PropertyInfo property, ContentImportLog log) : base(property, log)
        {
        }

        public override void Populate(AbstractEntity entity, Hashtable entityData, Type entityType)
        {
            if (Attribute.GetCustomAttribute(_property, typeof(FucineValue)) is FucineValue fucineValueAttr)
            {
                _property.SetValue(entity, fucineValueAttr.DefaultValue);
            }

            else 
            {
                Type type = _property.PropertyType;
                _property.SetValue(entity, Activator.CreateInstance(type));
            }
        }
    }
}