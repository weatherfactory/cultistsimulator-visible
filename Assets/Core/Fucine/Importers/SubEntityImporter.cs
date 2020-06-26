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

            //If no value can be found, initialise the property with a default instance of the correct type, then return
            if (hsubEntityHashtable==null)
            {
                Type type = _cachedFucinePropertyToPopulate.ThisPropInfo.PropertyType;
                _cachedFucinePropertyToPopulate.ThisPropInfo.SetValue(entity, Activator.CreateInstance(type));
                return false;
            }

            var subEntityAttribute = _cachedFucinePropertyToPopulate.FucineAttribute as FucineSubEntity;

         //   FucinePropertyWalker emanationWalker = new FucinePropertyWalker(Log, subEntityAttribute.ObjectType);

         var subEntity = Activator.CreateInstance(subEntityAttribute.ObjectType, hsubEntityHashtable, log);

                //emanationWalker.PopulateEntityWith(hsubEntityHashtable);


            _cachedFucinePropertyToPopulate.SetValueFastInvoke(entity, subEntity);

            return true;
        }
    }
}