using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Core.Interfaces;
using OrbCreationExtensions;

namespace Assets.Core.Fucine
{

    
    public class DictImporter:AbstractImporter
    {

        public override bool TryImport<T>(T entity, CachedFucineProperty<T> _cachedFucinePropertyToPopulate, Hashtable entityData,
            Type entityType, ContentImportLog log)
        {
            //If no value can be found, initialise the property with a default instance of the correct type, then return
            Hashtable hSubEntity = entityData.GetHashtable(_cachedFucinePropertyToPopulate.LowerCaseName);


            if (hSubEntity == null)
            {
                Type typeForDefaultSubEntity = _cachedFucinePropertyToPopulate.ThisPropInfo.PropertyType;
                _cachedFucinePropertyToPopulate.ThisPropInfo.SetValue(entity, WIPFactory.CreateObjectWithDefaultConstructor(typeForDefaultSubEntity));
                return false;
            }


            //possibilities:
            //Dictionary<string,string> 
            //Dictionary<string,int> 
            //Dictionary<string,T>
            //Dictionary<string,List<T>> where T is an IEntityUnique (and might be a QuickSpecEntity)
            //Dictionary<string,List<T>> where T is an IEntityAnonymous (and might be a QuickSpecEntity)

            //Check for / warn against any dictionaries without string as a key.


            var dictAttribute = _cachedFucinePropertyToPopulate.FucineAttribute as FucineDict;
            var entityProperties = entityType.GetProperties();



            //a hashtable of <id: listofmorphdetails>
            //eg, {fatiguing:husk} or eg: {fatiguing:[{id:husk,morpheffect:spawn},{id:smoke,morpheffect:spawn}],exiling:[{id:exiled,morpheffect:mutate},{id:liberated,morpheffect:mutate}]}
            Type dictType = _cachedFucinePropertyToPopulate.ThisPropInfo.PropertyType;
            Type dictMemberType = dictType.GetGenericArguments()[1];


            IDictionary dict = Activator.CreateInstance(dictType) as IDictionary;

            if (dictMemberType == typeof(string))
            {
                PopulateAsDictionaryOfStrings(entity, _cachedFucinePropertyToPopulate, hSubEntity, dict, log);
            }
            else if (dictMemberType == typeof(int))
            {
                PopulateAsDictionaryOfInts(entity, _cachedFucinePropertyToPopulate, hSubEntity, dict, log);
            }


            else if (dictMemberType.IsGenericType && dictMemberType.GetGenericTypeDefinition() == typeof(List<>))
            {
                PopulateAsDictionaryOfLists(entity, _cachedFucinePropertyToPopulate, dictMemberType, hSubEntity, dict,log);
            }


            else //it's an entity, not a string or a list
            {
                PopulateAsDictionaryOfEntities(entity, _cachedFucinePropertyToPopulate, hSubEntity, dictMemberType, dict, log);
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
                            Log.LogProblem(
                                $"{entity.GetType().Name} insists that {_cachedFucinePropertyToPopulate.LowerCaseName} should exist in {mustExistInProperty}, but that property is empty.");

                        if (!acceptableKeys.Contains(key))
                            Log.LogProblem(
                                $"{entity.GetType().Name} insists that {_cachedFucinePropertyToPopulate.LowerCaseName} should exist in {mustExistInProperty}, but the key {key} doesn't.");
                    }
                }
                else
                {
                    Log.LogProblem(
                        $"{entity.GetType().Name} insists that {_cachedFucinePropertyToPopulate.LowerCaseName} should exist in {dictAttribute.KeyMustExistIn}, but that property doesn't exist.");
                }
            }

            return true;
        }

        public void PopulateAsDictionaryOfStrings<T>(AbstractEntity<T> entity, CachedFucineProperty<T> _cachedFucinePropertyToPopulate, Hashtable subHashtable, IDictionary dictionary,ContentImportLog log) where T:AbstractEntity<T>
        {
            //Dictionary<string,string> - like DrawMessages
                foreach (DictionaryEntry de in subHashtable)
                {
                    dictionary.Add(de.Key, de.Value);
                }

                _cachedFucinePropertyToPopulate.ThisPropInfo.SetValue(entity, dictionary);
        }

        public void PopulateAsDictionaryOfInts<T>(AbstractEntity<T> entity, CachedFucineProperty<T> _cachedFucinePropertyToPopulate, Hashtable subHashtable, IDictionary dictionary, ContentImportLog log) where T : AbstractEntity<T>
        {
            //Dictionary<string,int> - like HaltVerbs
            foreach (DictionaryEntry de in subHashtable)
            {
                int value = Int32.Parse(de.Value.ToString());
                dictionary.Add(de.Key, value);
            }

            _cachedFucinePropertyToPopulate.ThisPropInfo.SetValue(entity,dictionary);
        }


        private void PopulateAsDictionaryOfLists<T>(T entity, CachedFucineProperty<T> _cachedFucinePropertyToPopulate, Type wrapperListType, Hashtable subHashtable, IDictionary dict, ContentImportLog log) where T: AbstractEntity<T>
        {
            //if Dictionary<T,List<T>> where T: entity then first create a wrapper list, then populate it with the individual entities //List<MorphDetails>, yup
            Type listMemberType = wrapperListType.GetGenericArguments()[0];
            //if it's {fatiguing:husk}, then it's a hashtable. If it's {fatiguing:[{id:husk,morpheffect:spawn},{id:smoke,morpheffect:spawn}], then it's also a hashtable.
            //either way, it's implicit keys: fatiguing, exiling... 
            foreach (string dictKeyForList in subHashtable.Keys)
            {
                IList wrapperList = WIPFactory.CreateObjectWithDefaultConstructor(wrapperListType) as IList;

                //if it's potentially a QuickSpecEntity 
                if (listMemberType.GetInterfaces().Contains(typeof(IQuickSpecEntity)) && (subHashtable[dictKeyForList] is string quickSpecEntityValue))
                {
                    //quick spec entities started out as a simple key:value pair, e.g. {fatiguing:husk}, but later had their possible definition extended to be potentially more complex, e.g fatiguing:[{id:husk,morpheffect:spawn},{id:smoke,morpheffect:spawn}]
                    //it's a quick spec entity if (i) it implements IQuickSpecEntity (ii) the value resolves to a string (rather than a list)
                    AddQuickSpecEntityToWrapperList(listMemberType, quickSpecEntityValue, wrapperList,log);
                }

                //either it's not a quickspeccable entity, or it's a quickspeccableentity whose json resolves to a full list 
                else if (subHashtable[dictKeyForList] is ArrayList list
                ) //fatiguing:[{id:husk,morpheffect:spawn},{id:smoke,morpheffect:spawn}]
                {
                    AddFullSpecEntitiesToWrapperList(list, listMemberType, wrapperList,log);
                }
                else
                {
                    throw new ApplicationException(
                        $"FucineDictionary {_cachedFucinePropertyToPopulate.LowerCaseName} on {entity.GetType().Name} is a List<T>, but the <T> isn't drawing from strings or hashtables, but rather a {subHashtable[dictKeyForList].GetType().Name}");
                }

                dict.Add(dictKeyForList, wrapperList); //{fatiguing:[{id:husk,morpheffect:spawn},{id:smoke,morpheffect:spawn}]
            }

            _cachedFucinePropertyToPopulate.ThisPropInfo.SetValue(entity, dict);
        }

        private static void AddQuickSpecEntityToWrapperList(Type listMemberType, string quickSpecEntityValue,
            IList wrapperList, ContentImportLog log)
        {
            // eg {fatiguing:husk}
            IQuickSpecEntity quickSpecEntity = Activator.CreateInstance(listMemberType) as IQuickSpecEntity;
            quickSpecEntity.QuickSpec(quickSpecEntityValue);
            wrapperList.Add(
                quickSpecEntity); //this is just the value/effect, eg :husk, wrapped up in a more complex object in a list. So the list will only contain this one object

        }

        private void AddFullSpecEntitiesToWrapperList(ArrayList list, Type listMemberType, IList wrapperList, ContentImportLog log)
        {
            foreach (Hashtable entityHash in list)
            {
                IEntityWithId
                    sub =WIPFactory.CreateEntity(listMemberType, entityHash, log);

                wrapperList.Add(sub);
            }

            //list is now: [{ id: husk,morpheffect: spawn}, {id: smoke,morpheffect: spawn}]
        }



        private void PopulateAsDictionaryOfEntities<T>(T entity, CachedFucineProperty<T> _cachedFucinePropertyToPopulate,Hashtable subHashtable, Type dictMemberType, IDictionary dict,ContentImportLog log) where T:AbstractEntity<T>
        {
            foreach (object o in subHashtable)
            {
                if (o is Hashtable h) //if the arraylist contains hashtables, then it contains subentities / emanations
                {
                //{fatiguing:[{id:husk,morpheffect:spawn},{id:smoke,morpheffect:spawn}]
                    IEntityWithId sub = WIPFactory.CreateEntity(dictMemberType, h, log);
                    dict.Add(sub.Id, sub);
                    _cachedFucinePropertyToPopulate.ThisPropInfo.SetValue(entity, dict);
                }
                else
                {
                    //we would hit this branch with subentities, like Expulsion, that don't have an id of their own
                    throw new ApplicationException(
                        $"FucineDictionary {_cachedFucinePropertyToPopulate.LowerCaseName} on {entity.GetType().Name} isn't a List<T>, a string, or drawing from a hashtable / IEntity - we don't know how to treat a {o.GetType().Name}");
                }
            }
        }
    }
}