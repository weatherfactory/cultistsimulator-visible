using System;
using System.Collections;
using System.Reflection;
using Assets.Core.Interfaces;
using OrbCreationExtensions;

namespace Assets.Core.Fucine
{
    public class ListImporter : AbstractImporter
    {



        public override bool TryImport<T>(T entity, CachedFucineProperty<T> _cachedFucinePropertyToPopulate, Hashtable entityData, Type entityType, ContentImportLog log)
        {

            ArrayList al = entityData.GetArrayList(_cachedFucinePropertyToPopulate.LowerCaseName);

            //If no value can be found, initialise the property with a default instance of the correct type, then return
            if (al==null)
            {
                Type type = _cachedFucinePropertyToPopulate.ThisPropInfo.PropertyType;
                _cachedFucinePropertyToPopulate.ThisPropInfo.SetValue(entity, Activator.CreateInstance(type));
                return false;
            }


            Type propertyListType = _cachedFucinePropertyToPopulate.ThisPropInfo.PropertyType;
            Type listMemberType = propertyListType.GetGenericArguments()[0];


            IList list = Activator.CreateInstance(propertyListType) as IList;

            _cachedFucinePropertyToPopulate.ThisPropInfo.SetValue(entity, list);

            foreach (var o in al)
            {

                if (o is Hashtable h) //if the arraylist contains hashtables, then it contains subentities / emanations
                {
                    
                    FucinePropertyWalker emanationWalker = new FucinePropertyWalker(Log, listMemberType);
                    var subEntity = Activator.CreateInstance(listMemberType, h, log);
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