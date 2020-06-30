using System;
using System.Collections;
using System.Reflection;
using Assets.Core.Interfaces;
using OrbCreationExtensions;

namespace Assets.Core.Fucine
{
    public class SubEntityImporter : AbstractImporter
    {

        public override bool TryImport<T>(T entity, CachedFucineProperty<T> _cachedFucinePropertyToPopulate, Hashtable entityData, Type entityType, ContentImportLog log)
        {
            string entityPropertyName = _cachedFucinePropertyToPopulate.LowerCaseName;
            var hsubEntityHashtable = entityData.GetHashtable(entityPropertyName);
            IEntityWithId subEntity;

            //If no value can be found, initialise the property with a default instance of the correct type, then return
            if (hsubEntityHashtable==null)
            {
                Type type = _cachedFucinePropertyToPopulate.ThisPropInfo.PropertyType;
                subEntity = FucineEntityFactory.CreateEntity(type, new Hashtable(), log);
                _cachedFucinePropertyToPopulate.ThisPropInfo.SetValue(entity, subEntity);
                return false;
            }

            if(_cachedFucinePropertyToPopulate.FucineAttribute is FucineSubEntity subEntityAttribute)
            {
                subEntity = FucineEntityFactory.CreateEntity(subEntityAttribute.ObjectType, hsubEntityHashtable, log);
                _cachedFucinePropertyToPopulate.SetValueFastInvoke(entity, subEntity);
            }
            else
            
             log.LogProblem($"Tried to import an subentity for property {_cachedFucinePropertyToPopulate.LowerCaseName} on entity {entity.GetType().Name}, but it isn't marked with FucineSubEntity");

            return true;
        }
    }
}