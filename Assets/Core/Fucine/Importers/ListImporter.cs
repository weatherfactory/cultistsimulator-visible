using System;
using System.Collections;
using System.Reflection;
using Assets.Core.Interfaces;
using OrbCreationExtensions;

namespace Assets.Core.Fucine
{
    public class ListImporter : AbstractImporter
    {

        public ListImporter(CachedFucineProperty property, ContentImportLog log) : base(property, log)
        {
        }


        public override bool TryImport(AbstractEntity entity, Hashtable entityData, Type entityType)
        {

            ArrayList al = entityData.GetArrayList(_cachedFucinePropertyToPopulate.LowerCaseName);

            //If no value can be found, initialise the property with a default instance of the correct type, then return
            if (al==null)
            {
                Type type = _cachedFucinePropertyToPopulate.PropertyInfo.PropertyType;
                _cachedFucinePropertyToPopulate.PropertyInfo.SetValue(entity, Activator.CreateInstance(type));
                return false;
            }


            Type propertyListType = _cachedFucinePropertyToPopulate.PropertyInfo.PropertyType;
            Type listMemberType = propertyListType.GetGenericArguments()[0];


            IList list = Activator.CreateInstance(propertyListType) as IList;

            _cachedFucinePropertyToPopulate.PropertyInfo.SetValue(entity, list);

            foreach (var o in al)
            {

                if (o is Hashtable h) //if the arraylist contains hashtables, then it contains subentities / emanations
                {
                    
                    FucinePropertyWalker emanationWalker = new FucinePropertyWalker(Log, listMemberType);
                    var subEntity = emanationWalker.PopulateEntityWith(h);
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