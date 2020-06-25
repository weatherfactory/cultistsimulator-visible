using System;
using System.Collections;
using System.Reflection;
using Assets.Core.Interfaces;

namespace Assets.Core.Fucine
{
    public class FucineImportDefault : FucineImport
    {
        public FucineImportDefault(CachedFucineProperty cachedFucinePropertyToPopulate, ContentImportLog log) : base(cachedFucinePropertyToPopulate, log)
        {
        }

        public override void Populate(AbstractEntity entity, Hashtable entityData, Type entityType)
        {
            if (_cachedFucinePropertyToPopulate.FucineAttribute is FucineValue fucineValueAttr)
            {
                _cachedFucinePropertyToPopulate.PropertyInfo.SetValue(entity, fucineValueAttr.DefaultValue);
            }

            else 
            {
                Type type = _cachedFucinePropertyToPopulate.PropertyInfo.PropertyType;
                _cachedFucinePropertyToPopulate.PropertyInfo.SetValue(entity, Activator.CreateInstance(type));
            }
        }
    }
}