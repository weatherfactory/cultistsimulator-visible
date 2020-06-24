using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Core.Interfaces;
using OrbCreationExtensions;

namespace Assets.Core.Fucine
{

    
    public class FucineImportDict : FucineImport
    {

        public FucineImportDict(PropertyInfo property, ContentImportLogger logger) : base(property, logger)
        {
        }

        public void PopulateAsDictionaryOfStrings(Entity entity, Hashtable subHashtable, IDictionary dictionary)
        {
            //Dictionary<string,string> - like DrawMessages
                foreach (DictionaryEntry de in subHashtable)
                {
                    dictionary.Add(de.Key, de.Value);
                }

                _property.SetValue(entity, dictionary);
        }

        public void PopulateAsDictionaryOfInts(Entity entity, Hashtable subHashtable, IDictionary dictionary)
        {
            //Dictionary<string,int> - like HaltVerbs
            foreach (DictionaryEntry de in subHashtable)
            {
                int value = Int32.Parse(de.Value.ToString());
                dictionary.Add(de.Key, value);
            }

            _property.SetValue(entity,dictionary);
        }

        public override void Populate(Entity entity, Hashtable entityData, Type entityType)
        {
            //Dictionary<string,string> 
            //Dictionary<string,int> 
            //Dictionary<string,T>
            //Dictionary<string,List<T>> where T is an IEntityUnique (and might be a QuickSpecEntity)
            //Dictionary<string,List<T>> where T is an IEntityAnonymous (and might be a QuickSpecEntity)

            //Check for / warn against any dictionaries without string as a key.


            var dictAttribute = Attribute.GetCustomAttribute(_property, typeof(FucineDict)) as FucineDict;
            var entityProperties = entityType.GetProperties();

            Hashtable hSubEntity = entityData.GetHashtable(_property.Name);

            Hashtable cihSubEntity = System.Collections.Specialized.CollectionsUtil.CreateCaseInsensitiveHashtable(hSubEntity);  //a hashtable of <id: listofmorphdetails>
            //eg, {fatiguing:husk} or eg: {fatiguing:[{id:husk,morpheffect:spawn},{id:smoke,morpheffect:spawn}],exiling:[{id:exiled,morpheffect:mutate},{id:liberated,morpheffect:mutate}]}
            Type dictType = _property.PropertyType; 
            Type dictMemberType = dictType.GetGenericArguments()[1];


            IDictionary dict = Activator.CreateInstance(dictType) as IDictionary;

            if (dictMemberType == typeof(string))
            {
                PopulateAsDictionaryOfStrings(entity, cihSubEntity, dict);
            }
            else if (dictMemberType == typeof(int))
            {
                PopulateAsDictionaryOfInts(entity,cihSubEntity,dict);
            }

         
            else if (dictMemberType.IsGenericType && dictMemberType.GetGenericTypeDefinition() == typeof(List<>)) 
            {
                PopulateAsDictionaryOfLists(entity, dictMemberType, cihSubEntity, dict);
            }


            else //it's an entity, not a string or a list
            {
                PopulateAsDictionaryOfEntities(entity, cihSubEntity, dictMemberType, dict);
            }

          


            if (dictAttribute.KeyMustExistIn != null)
            {
                var mustExistInProperty =
                    entityProperties.SingleOrDefault(p => p.Name == dictAttribute.KeyMustExistIn);
                if (mustExistInProperty != null)
                {
                    foreach (var key in dict.Keys)
                    {
                        List<string> acceptableKeys =
                            mustExistInProperty.GetValue(entity) as List<string>;

                        if (acceptableKeys == null)
                            _logger.LogProblem(
                                $"{entity.GetType().Name} insists that {_property.Name} should exist in {mustExistInProperty}, but that property is empty.");

                        if (!acceptableKeys.Contains(key))
                            _logger.LogProblem(
                                $"{entity.GetType().Name} insists that {_property.Name} should exist in {mustExistInProperty}, but the key {key} doesn't.");
                    }
                }
                else
                {
                    _logger.LogProblem(
                        $"{entity.GetType().Name} insists that {_property.Name} should exist in {dictAttribute.KeyMustExistIn}, but that property doesn't exist.");
                }
            }
        }

        private void PopulateAsDictionaryOfLists(Entity entity, Type wrapperListType, Hashtable subHashtable, IDictionary dict)
        {
            //if Dictionary<T,List<T>> where T: entity then first create a wrapper list, then populate it with the individual entities //List<MorphDetails>, yup
            Type listMemberType = wrapperListType.GetGenericArguments()[0];
            //if it's {fatiguing:husk}, then it's a hashtable. If it's {fatiguing:[{id:husk,morpheffect:spawn},{id:smoke,morpheffect:spawn}], then it's also a hashtable.
            //either way, it's implicit keys: fatiguing, exiling... 
            foreach (string k in subHashtable.Keys)
            {
                IList wrapperList = Activator.CreateInstance(wrapperListType) as IList;
                if (listMemberType.GetInterfaces().Contains(typeof(IQuickSpecEntity)))
                {
                    AddQuickSpecEntityToWrapperList(subHashtable, listMemberType, k, wrapperList);
                }

                else if (subHashtable[k] is ArrayList list
                ) //fatiguing:[{id:husk,morpheffect:spawn},{id:smoke,morpheffect:spawn}]
                {
                    AddFullSpecEntitiesToWrapperList(list, listMemberType, wrapperList);
                }
                else
                {
                    throw new ApplicationException(
                        $"FucineDictionary {_property.Name} on {entity.GetType().Name} is a List<T>, but the <T> isn't drawing from strings or hashtables, but rather a {subHashtable[k].GetType().Name}");
                }

                dict.Add(k, wrapperList); //{fatiguing:[{id:husk,morpheffect:spawn},{id:smoke,morpheffect:spawn}]
            }

            _property.SetValue(entity, dict);
        }

        private static void AddQuickSpecEntityToWrapperList(Hashtable subHashtable, Type listMemberType, string k,
            IList wrapperList)
        {
            //{fatiguing:husk}
            IQuickSpecEntity quickSpecEntity = Activator.CreateInstance(listMemberType) as IQuickSpecEntity;
            quickSpecEntity.QuickSpec(subHashtable[k] as string);
            wrapperList.Add(
                quickSpecEntity); //this is just the value/effect, eg :husk, wrapped up in a more complex object in a list. So the list will only contain this one object

        }

        private void AddFullSpecEntitiesToWrapperList(ArrayList list, Type listMemberType, IList wrapperList)
        {
            foreach (Hashtable entityHash in list)
            {
                Hashtable ciEntityHash =
                    System.Collections.Specialized.CollectionsUtil.CreateCaseInsensitiveHashtable(
                        entityHash);

                FucinePropertyWalker
                    emanationWalker =
                        new FucinePropertyWalker(_logger, listMemberType); //passing in <string,MorphDetailsList>
                IEntityWithId
                    sub = emanationWalker
                        .PopulateEntityWith(ciEntityHash) as IEntityWithId; //{id:husk,morpheffect:spawn}
                wrapperList.Add(sub);
            }

            //list is now: [{ id: husk,morpheffect: spawn}, {id: smoke,morpheffect: spawn}]
        }



        private void PopulateAsDictionaryOfEntities(Entity entity, Hashtable subHashtable, Type dictMemberType, IDictionary dict)
        {
            foreach (object o in subHashtable)
            {
                if (o is Hashtable h) //if the arraylist contains hashtables, then it contains subentities / emanations
                {
                    Hashtable cih =
                        System.Collections.Specialized.CollectionsUtil
                            .CreateCaseInsensitiveHashtable(
                                h); //{fatiguing:[{id:husk,morpheffect:spawn},{id:smoke,morpheffect:spawn}]
                    FucinePropertyWalker
                        emanationWalker =
                            new FucinePropertyWalker(_logger, dictMemberType); //passing in <string,MorphDetailsList>
                    IEntityWithId sub = emanationWalker.PopulateEntityWith(cih) as IEntityWithId;
                    dict.Add(sub.Id, sub);
                    _property.SetValue(entity, dict);
                }
                else
                {
                    //we would hit this branch with subentities, like Expulsion, that don't have an id of their own
                    throw new ApplicationException(
                        $"FucineDictionary {_property.Name} on {entity.GetType().Name} isn't a List<T>, a string, or drawing from a hashtable / IEntity - we don't know how to treat a {o.GetType().Name}");
                }
            }
        }
    }
}