using System;
using System.Collections;
using System.Reflection;
using Assets.Core.Interfaces;
using OrbCreationExtensions;

namespace Assets.Core.Fucine
{
    public class FucineImportSubEntity : FucineImport
    {
        public FucineImportSubEntity(PropertyInfo property, ContentImportLogger logger) : base(property, logger)
        {
        }

        public override void Populate(AbstractEntity entity, Hashtable entityData, Type entityType)
        {
            var subEntityAttribute = Attribute.GetCustomAttribute(_property, typeof(FucineSubEntity)) as FucineSubEntity;


            string entityPropertyName = _property.Name;
            FucinePropertyWalker emanationWalker = new FucinePropertyWalker(_logger, subEntityAttribute.ObjectType);

            var subEntity = emanationWalker.PopulateEntityWith(entityData.GetHashtable(entityPropertyName));

            _property.SetValue(entity, subEntity);
        }
    }
}