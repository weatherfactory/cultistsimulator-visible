﻿using System;
using System.Collections;
using System.Reflection;
using Assets.Core.Interfaces;
using OrbCreationExtensions;

namespace Assets.Core.Fucine
{
    public class SubEntityImporter : AbstractImporter
    {

        public override bool TryImportProperty<T>(T entity, CachedFucineProperty<T> propertyToValidate, Hashtable entityData, ContentImportLog log)
        {
            string entityPropertyName = propertyToValidate.LowerCaseName;
            var hsubEntityHashtable = entityData.GetHashtable(entityPropertyName);
            IEntityWithId subEntity;

            //If no value can be found, initialise the property with a default instance of the correct type, then return
            if (hsubEntityHashtable==null)
            {
                Type type = propertyToValidate.ThisPropInfo.PropertyType;
                subEntity = FactoryInstantiator.CreateEntity(type, new Hashtable(), log);
                propertyToValidate.SetViaFastInvoke(entity, subEntity);
                return false;
            }

            if(propertyToValidate.FucineAttribute is FucineSubEntity subEntityAttribute)
            {
                subEntity = FactoryInstantiator.CreateEntity(subEntityAttribute.ObjectType, hsubEntityHashtable, log);
                propertyToValidate.SetViaFastInvoke(entity, subEntity);
            }
            else
            
             log.LogProblem($"Tried to import an subentity for property {propertyToValidate.LowerCaseName} on entity {entity.GetType().Name}, but it isn't marked with FucineSubEntity");

            return true;
        }
    }
}