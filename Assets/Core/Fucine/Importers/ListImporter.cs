using System;
using System.Collections;
using System.Reflection;
using Assets.Core.Interfaces;
using OrbCreationExtensions;

namespace Assets.Core.Fucine
{
    public class ListImporter : AbstractFucineImporter
    {

        public ListImporter(CachedFucineProperty property, ContentImportLog log) : base(property, log)
        {
        }


        public override void Populate(AbstractEntity entity, Hashtable entityData, Type entityType)
        {
            ArrayList al = entityData.GetArrayList(_cachedFucinePropertyToPopulate.Name);
            Type propertyListType = _cachedFucinePropertyToPopulate.PropertyInfo.PropertyType;
            Type listMemberType = propertyListType.GetGenericArguments()[0];


            IList list = Activator.CreateInstance(propertyListType) as IList;

            _cachedFucinePropertyToPopulate.PropertyInfo.SetValue(entity, list);

            foreach (var o in al)
            {

                if (o is Hashtable h) //if the arraylist contains hashtables, then it contains subentities / emanations
                {
                    Hashtable cih = System.Collections.Specialized.CollectionsUtil.CreateCaseInsensitiveHashtable(h);
                    FucinePropertyWalker emanationWalker = new FucinePropertyWalker(Log, listMemberType);
                    var subEntity = emanationWalker.PopulateEntityWith(cih);
                    list.Add(subEntity);
                }
                else
                {
                    list.Add(o); //This might not work for things that aren't strings?
                }
            }

        }
    }
}