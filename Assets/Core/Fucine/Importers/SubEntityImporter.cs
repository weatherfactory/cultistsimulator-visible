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

        public override void Populate(AbstractEntity entity, Hashtable entityData, Type entityType)
        {
            //If no value can be found, initialise the property with a default instance of the correct type, then return
            if (!entityData.ContainsKey(_cachedFucinePropertyToPopulate.Name))
            {
                Type type = _cachedFucinePropertyToPopulate.PropertyInfo.PropertyType;
                _cachedFucinePropertyToPopulate.PropertyInfo.SetValue(entity, Activator.CreateInstance(type));
                return;
            }

            var subEntityAttribute = _cachedFucinePropertyToPopulate.FucineAttribute as FucineSubEntity;


            string entityPropertyName = _cachedFucinePropertyToPopulate.Name;
            FucinePropertyWalker emanationWalker = new FucinePropertyWalker(Log, subEntityAttribute.ObjectType);

            var subEntity = emanationWalker.PopulateEntityWith(entityData.GetHashtable(entityPropertyName));

            _cachedFucinePropertyToPopulate.PropertyInfo.SetValue(entity, subEntity);
        }
    }
}