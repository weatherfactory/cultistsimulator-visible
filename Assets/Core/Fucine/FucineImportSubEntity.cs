using System;
using System.Collections;
using System.Reflection;
using Assets.Core.Interfaces;
using OrbCreationExtensions;

namespace Assets.Core.Fucine
{
    public class FucineImportSubEntity : FucineImport
    {
        public FucineImportSubEntity(PropertyInfo property, ContentImportLog log) : base(property, log)
        {
        }

        public override void Populate(AbstractEntity entity, Hashtable entityData, Type entityType)
        {
            var subEntityAttribute = Attribute.GetCustomAttribute(_property, typeof(FucineSubEntity)) as FucineSubEntity;


            string entityPropertyName = _property.Name;
            FucinePropertyWalker emanationWalker = new FucinePropertyWalker(Log, subEntityAttribute.ObjectType);

            var subEntity = emanationWalker.PopulateEntityWith(entityData.GetHashtable(entityPropertyName));

            _property.SetValue(entity, subEntity);
        }
    }
}