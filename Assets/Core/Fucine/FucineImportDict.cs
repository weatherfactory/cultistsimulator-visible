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

        public override void Populate(IEntity entity, Hashtable entityData, Type entityType)
        {
            var dictAttribute = Attribute.GetCustomAttribute(_property, typeof(FucineDict)) as FucineDict;
            var entityProperties = entityType.GetProperties();

            Hashtable subHashtable = System.Collections.Specialized.CollectionsUtil.CreateCaseInsensitiveHashtable(entityData.GetHashtable(_property.Name));  //a hashtable of <id: listofmorphdetails>
            //eg, {fatiguing:husk} or eg: {fatiguing:[{id:husk,morpheffect:spawn},{id:smoke,morpheffect:spawn}],exiling:[{id:exiled,morpheffect:mutate},{id:liberated,morpheffect:mutate}]}
            Type dictType = _property.PropertyType; //Dictionary<string,List<MorphDetails>
            Type dictMemberType = dictType.GetGenericArguments()[1]; //List<MorphDetails>


            IDictionary dict = Activator.CreateInstance(dictType) as IDictionary; //Dictionary<string,MorphDetailsList>

            //if Dictionary<T,List<T>> where T: entity then create that list, then populate it with the individual entities 
            if (dictMemberType.IsGenericType && dictMemberType.GetGenericTypeDefinition() == typeof(List<>)) //List<MorphDetails>, yup
            {

                Type wrapperListMemberType = dictMemberType.GetGenericArguments()[0];
                //if it's {fatiguing:husk}, then it's a hashtable. If it's {fatiguing:[{id:husk,morpheffect:spawn},{id:smoke,morpheffect:spawn}], then it's also a hashtable.
                //either way, it's arbitrary keys: fatiguing, exiling... ie it's an IEntityAnonymous, but it might also be a QuickSpecEntity
                foreach (string k in subHashtable.Keys)
                {
                    IList wrapperList = Activator.CreateInstance(dictMemberType) as IList;
                    if (subHashtable[k] is string value && wrapperListMemberType.GetInterfaces().Contains(typeof(IQuickSpecEntity)))
                    {
                        //{fatiguing:husk}
                        IQuickSpecEntity quickSpecEntity = Activator.CreateInstance(wrapperListMemberType) as IQuickSpecEntity;
                        quickSpecEntity.QuickSpec(value);
                        wrapperList.Add(quickSpecEntity); //this is just the value/effect, eg :husk, wrapped up in a more complex object in a list. So the list will only contain this one object
                        dict.Add(k, wrapperList);
                    }

                    else if (subHashtable[k] is ArrayList list) //fatiguing:[{id:husk,morpheffect:spawn},{id:smoke,morpheffect:spawn}]
                    {
                        foreach (Hashtable entityHash in list)
                        {
                            Hashtable ciEntityHash =
                                System.Collections.Specialized.CollectionsUtil.CreateCaseInsensitiveHashtable(
                                    entityHash);

                            FucinePropertyWalker emanationWalker = new FucinePropertyWalker(_logger, wrapperListMemberType); //passing in <string,MorphDetailsList>
                            IEntityUnique sub = emanationWalker.PopulateEntityWith(ciEntityHash) as IEntityUnique; //{id:husk,morpheffect:spawn}
                            wrapperList.Add(sub);
                        }
                        //list is now: [{ id: husk,morpheffect: spawn}, {id: smoke,morpheffect: spawn}]

                        dict.Add(k, wrapperList); //{fatiguing:[{id:husk,morpheffect:spawn},{id:smoke,morpheffect:spawn}]

                    }
                    else
                    {
                        throw new ApplicationException($"FucineDictionary {_property.Name} on {entity.GetType().Name} is a List<T>, but the <T> isn't drawing from strings or hashtables, but rather a {subHashtable[k].GetType().Name}");
                    }
                }


            }

            //Dictionary<T,string> - like DrawMessages
            else if (dictMemberType == typeof(string)) //nope, it's MorphDetailsList, so we never see this branch
            {
                foreach (DictionaryEntry de in subHashtable)
                {
                    dict.Add(de.Key, de.Value);
                }

            }
            else //it's an entity, not a string or a list
            {

                foreach (object o in subHashtable)
                {

                    if (o is Hashtable h) //if the arraylist contains hashtables, then it contains subentities / emanations
                    {
                        Hashtable cih = System.Collections.Specialized.CollectionsUtil.CreateCaseInsensitiveHashtable(h); //{fatiguing:[{id:husk,morpheffect:spawn},{id:smoke,morpheffect:spawn}]
                        FucinePropertyWalker emanationWalker = new FucinePropertyWalker(_logger, dictMemberType); //passing in <string,MorphDetailsList>
                        IEntityUnique sub = emanationWalker.PopulateEntityWith(cih) as IEntityUnique;
                        dict.Add(sub.Id, sub);

                    }
                    else
                    {
                        //we would hit this branch with subentities, like Expulsion, that don't have an id of their own
                        throw new ApplicationException($"FucineDictionary {_property.Name} on {entity.GetType().Name} isn't a List<T>, a string, or drawing from a hashtable / IEntity - we don't know how to treat a {o.GetType().Name}");
                    }


                }

            }

            _property.SetValue(entity, dict);


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
    }
}