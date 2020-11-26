using System;
using System.Collections;
using System.Reflection;
using Assets.Core.Fucine.DataImport;
using Assets.Core.Interfaces;
using OrbCreationExtensions;

namespace Assets.Core.Fucine
{
    public class SubEntityImporter : AbstractImporter
    {

        public override bool TryImportProperty<T>(T entity, CachedFucineProperty<T> propertyToValidate, EntityData entityData, ContentImportLog log)
        {
            string entityPropertyName = propertyToValidate.LowerCaseName;
           EntityData subEntityData = entityData.ValuesTable[entityPropertyName] as EntityData;
            IEntityWithId subEntity;

            //If no value can be found, initialise the property with a default instance of the correct type, then return
            if (subEntityData == null)
            {
                Type type = propertyToValidate.ThisPropInfo.PropertyType;
                subEntity = FactoryInstantiator.CreateObjectWithDefaultConstructor(type) as IEntityWithId;

                //subEntity = FactoryInstantiator.CreateEntity(type, new EntityData(), log);
                propertyToValidate.SetViaFastInvoke(entity, subEntity);
                return false;
            }

            if(propertyToValidate.FucineAttribute is FucineSubEntity subEntityAttribute)
            {
                subEntity = FactoryInstantiator.CreateEntity(subEntityAttribute.ObjectType, subEntityData, log);
                propertyToValidate.SetViaFastInvoke(entity, subEntity);
            }
            else
            
             log.LogProblem($"Tried to import an subentity for property {propertyToValidate.LowerCaseName} on entity {entity.GetType().Name}, but it isn't marked with FucineSubEntity");

            return true;
        }
    }
}