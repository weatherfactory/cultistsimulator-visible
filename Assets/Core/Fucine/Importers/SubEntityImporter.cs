using System;
using System.Collections;
using System.Reflection;
using Assets.Core.Interfaces;
using OrbCreationExtensions;

namespace Assets.Core.Fucine
{
    public class SubEntityImporter : AbstractFucineImporter
    {
        public SubEntityImporter(CachedFucineProperty cachedFucinePropertyToPopulate, ContentImportLog log) : base(cachedFucinePropertyToPopulate, log)
        {
        }

        public override void Populate(AbstractEntity entity, Hashtable entityData, Type entityType)
        {
            var subEntityAttribute = _cachedFucinePropertyToPopulate.FucineAttribute as FucineSubEntity;


            string entityPropertyName = _cachedFucinePropertyToPopulate.Name;
            FucinePropertyWalker emanationWalker = new FucinePropertyWalker(Log, subEntityAttribute.ObjectType);

            var subEntity = emanationWalker.PopulateEntityWith(entityData.GetHashtable(entityPropertyName));

            _cachedFucinePropertyToPopulate.PropertyInfo.SetValue(entity, subEntity);
        }
    }
}