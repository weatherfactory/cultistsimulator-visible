using System;
using System.Collections;
using System.Reflection;
using Assets.Core.Interfaces;
using OrbCreationExtensions;

namespace Assets.Core.Fucine
{
    public class ListImporter : AbstractImporter
    {



        public override bool TryImportProperty<T>(T entity, CachedFucineProperty<T> _cachedFucinePropertyToPopulate, Hashtable entityData, ContentImportLog log)
        {

            ArrayList al = entityData.GetArrayList(_cachedFucinePropertyToPopulate.LowerCaseName);

            //If no value can be found, initialise the property with a default instance of the correct type, then return
            if (al==null)
            {
                Type propertyType = _cachedFucinePropertyToPopulate.ThisPropInfo.PropertyType;
                _cachedFucinePropertyToPopulate.SetViaFastInvoke(entity, WIPFactory.CreateObjectWithDefaultConstructor(propertyType));
                return false;
            }


            Type propertyListType = _cachedFucinePropertyToPopulate.ThisPropInfo.PropertyType;
            Type listMemberType = propertyListType.GetGenericArguments()[0];

            IList list = WIPFactory.CreateObjectWithDefaultConstructor(propertyListType) as IList;

            _cachedFucinePropertyToPopulate.SetViaFastInvoke(entity, list);

            foreach (var o in al)
            {

                if (o is Hashtable h) //if the arraylist contains hashtables, then it contains subentities / emanations
                {
                    var subEntity = WIPFactory.CreateEntity(listMemberType, h, log);
                    list.Add(subEntity);
                }
                else
                {
                    list.Add(o); //This might not work for things that aren't strings?
                }
            }

            return true;
        }
    }
}