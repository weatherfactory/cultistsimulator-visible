using System;
using System.Collections;
using System.Reflection;
using Assets.Core.Interfaces;
using OrbCreationExtensions;

namespace Assets.Core.Fucine
{
    public class SubEntityImporter : AbstractImporter
    {
        public SubEntityImporter(CachedFucineProperty cachedFucinePropertyToPopulate, ContentImportLog log) : base(cachedFucinePropertyToPopulate, log)
        {
        }

        public override bool TryImport(AbstractEntity entity, Hashtable entityData, Type entityType)
        {
            string entityPropertyName = _cachedFucinePropertyToPopulate.LowerCaseName;
            var hsubEntityHashtable = entityData.GetHashtable(entityPropertyName);

            //If no value can be found, initialise the property with a default instance of the correct type, then return
            if (hsubEntityHashtable==null)
            {
                Type type = _cachedFucinePropertyToPopulate.PropertyInfo.PropertyType;
                _cachedFucinePropertyToPopulate.PropertyInfo.SetValue(entity, Activator.CreateInstance(type));
                return false;
            }

            var subEntityAttribute = _cachedFucinePropertyToPopulate.FucineAttribute as FucineSubEntity;

            FucinePropertyWalker emanationWalker = new FucinePropertyWalker(Log, subEntityAttribute.ObjectType);

            var subEntity = emanationWalker.PopulateEntityWith(hsubEntityHashtable);

            _cachedFucinePropertyToPopulate.PropertyInfo.SetValue(entity, subEntity);

            return true;
        }
    }
}