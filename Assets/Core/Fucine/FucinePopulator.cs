using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Interfaces;
using OrbCreationExtensions;

namespace Assets.Core.Fucine
{
   public class FucinePopulator
    {
        private readonly IEntity _entity;
        private readonly Hashtable _entityData;
        private readonly ContentImportLogger _logger;
        private readonly Type _entityType;

        public FucinePopulator(IEntity entity, Hashtable entityData,ContentImportLogger logger,Type entityType)
        {
            _entity = entity;
            _entityData = entityData;
            _logger = logger;
            _entityType = entityType;
        }

        public void PopulateProperty(Hashtable htEntityValues, PropertyInfo entityProperty, dynamic entityToPopulate,
                   PropertyInfo[] entityProperties)
        {

            if (Attribute.GetCustomAttribute(entityProperty, typeof(FucineId)) is FucineId idProp)
            {
                if (htEntityValues.ContainsKey(entityProperty.Name))
                    entityProperty.SetValue(entityToPopulate, htEntityValues.GetValue(entityProperty.Name));
                else
                {
                    _logger.LogProblem("ID not specified for a " + _entityType.Name);
                }
            }

            else if (Attribute.GetCustomAttribute(entityProperty, typeof(FucineList)) is
                FucineList)
            {
                PopulateList(htEntityValues, entityProperty, entityToPopulate);
            }

            else if (Attribute.GetCustomAttribute(entityProperty, typeof(FucineDict)) is
                FucineDict dictAttribute)
            {
                PopulateDict(htEntityValues, entityProperty, entityToPopulate, dictAttribute, entityProperties);
            }


            else if (Attribute.GetCustomAttribute(entityProperty, typeof(FucineAspects)) is
                FucineAspects
                aspectsProp)
            {
                PopulateAspectsDictionary(htEntityValues, entityProperty, entityToPopulate, aspectsProp,
                    entityProperties);
            }

            else if (Attribute.GetCustomAttribute(entityProperty, typeof(FucineSubEntity)) is
                FucineSubEntity objectProp)
            {
                PopulateSubEntityProperty(htEntityValues, entityProperty, entityToPopulate, objectProp);
            }

            else
            {
                TypeConverter typeConverter = TypeDescriptor.GetConverter(entityProperty.PropertyType);

                entityProperty.SetValue(entityToPopulate, typeConverter.ConvertFromString(htEntityValues[entityProperty.Name].ToString()));
            }

        }



        private void PopulateList(Hashtable htEntityValues, PropertyInfo entityProperty, object entityToPopulate)
        {

            ArrayList al = htEntityValues.GetArrayList(entityProperty.Name);
            Type propertyListType = entityProperty.PropertyType;
            Type listMemberType = propertyListType.GetGenericArguments()[0];


            IList list = Activator.CreateInstance(propertyListType) as IList;

            entityProperty.SetValue(entityToPopulate, list);

            foreach (var o in al)
            {

                if (o is Hashtable h) //if the arraylist contains hashtables, then it contains subentities / emanations
                {
                    Hashtable cih = System.Collections.Specialized.CollectionsUtil.CreateCaseInsensitiveHashtable(h);
                    FucinePropertyWalker emanationWalker = new FucinePropertyWalker(_logger, listMemberType);
                    var subEntity = emanationWalker.PopulateEntityWith(cih);
                    list.Add(subEntity);
                }
                else
                {
                    list.Add(o); //This might not work for things that aren't strings?
                }
            }


        }


        private void PopulateDict(Hashtable htAllEntityValues, PropertyInfo entityProperty, object entityToPopulate, FucineDict dictAttribute, PropertyInfo[] entityProperties)
        {

            Hashtable subHashtable = System.Collections.Specialized.CollectionsUtil.CreateCaseInsensitiveHashtable(htAllEntityValues.GetHashtable(entityProperty.Name));  //a hashtable of <id: listofmorphdetails>
            //eg, {fatiguing:husk} or eg: {fatiguing:[{id:husk,morpheffect:spawn},{id:smoke,morpheffect:spawn}],exiling:[{id:exiled,morpheffect:mutate},{id:liberated,morpheffect:mutate}]}
            Type dictType = entityProperty.PropertyType; //Dictionary<string,List<MorphDetails>
            Type dictMemberType = dictType.GetGenericArguments()[1]; //List<MorphDetails>


            IDictionary dict = Activator.CreateInstance(dictType) as IDictionary; //Dictionary<string,MorphDetailsList>

            //if dictMemberType is a list then create that list, then populate it with the individual entities 
            if (dictMemberType.IsGenericType && dictMemberType.GetGenericTypeDefinition() == typeof(List<>)) //List<MorphDetails>, yup
            {

                Type wrapperListMemberType = dictMemberType.GetGenericArguments()[0];
                //if it's {fatiguing:husk}, then it's a hashtable. If it's {fatiguing:[{id:husk,morpheffect:spawn},{id:smoke,morpheffect:spawn}], then it's also a hashtable.
                //either way, it's arbitrary keys: fatiguing, exiling...
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
                            IEntityKeyed sub = emanationWalker.PopulateEntityWith(ciEntityHash) as IEntityKeyed; //{id:husk,morpheffect:spawn}
                            wrapperList.Add(sub);
                        }
                        //list is now: [{ id: husk,morpheffect: spawn}, {id: smoke,morpheffect: spawn}]

                        dict.Add(k, wrapperList); //{fatiguing:[{id:husk,morpheffect:spawn},{id:smoke,morpheffect:spawn}]

                    }
                    else
                    {
                        throw new ApplicationException($"FucineDictionary {entityProperty.Name} on {entityToPopulate.GetType().Name} is a List<T>, but the <T> isn't drawing from strings or hashtables, but rather a {subHashtable[k].GetType().Name}");
                    }
                }


            }




            //always and ever a string/string proposition - like DrawMessages
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
                        IEntityKeyed sub = emanationWalker.PopulateEntityWith(cih) as IEntityKeyed;
                        dict.Add(sub.Id, sub);

                    }
                    else
                    {
                        //we would hit this branch with subentities, like Expulsion, that don't have an id of their own
                        throw new ApplicationException($"FucineDictionary {entityProperty.Name} on {entityToPopulate.GetType().Name} isn't a List<T>, a string, or drawing from a hashtable / IEntity - we don't know how to treat a {o.GetType().Name}");
                    }


                }

            }

            entityProperty.SetValue(entityToPopulate, dict);


            if (dictAttribute.KeyMustExistIn != null)
            {
                var mustExistInProperty =
                    entityProperties.SingleOrDefault(p => p.Name == dictAttribute.KeyMustExistIn);
                if (mustExistInProperty != null)
                {
                    foreach (var key in dict.Keys)
                    {
                        List<string> acceptableKeys =
                            mustExistInProperty.GetValue(entityToPopulate) as List<string>;

                        if (acceptableKeys == null)
                            _logger.LogProblem(
                                $"{entityToPopulate.GetType().Name} insists that {entityProperty.Name} should exist in {mustExistInProperty}, but that property is empty.");

                        if (!acceptableKeys.Contains(key))
                            _logger.LogProblem(
                                $"{entityToPopulate.GetType().Name} insists that {entityProperty.Name} should exist in {mustExistInProperty}, but the key {key} doesn't.");
                    }
                }
                else
                {
                    _logger.LogProblem(
                        $"{entityToPopulate.GetType().Name} insists that {entityProperty.Name} should exist in {dictAttribute.KeyMustExistIn}, but that property doesn't exist.");
                }
            }

        }

        private void PopulateSubEntityProperty(Hashtable htEntityValues, PropertyInfo entityProperty, IEntityKeyed entityKeyedToPopulate, FucineSubEntity subEntityProp)
        {

            string entityPropertyName = entityProperty.Name;
            FucinePropertyWalker emanationWalker = new FucinePropertyWalker(_logger, subEntityProp.ObjectType);

            var subEntity = emanationWalker.PopulateEntityWith(htEntityValues.GetHashtable(entityPropertyName));

            entityProperty.SetValue(entityKeyedToPopulate, subEntity);

        }


        private void PopulateAspectsDictionary(Hashtable htEntityValues, PropertyInfo entityProperty,
            IEntityKeyed entityKeyedToPopulate,
            FucineAspects aspectsProp, PropertyInfo[] entityProperties)
        {
            var htEntries = htEntityValues.GetHashtable(entityProperty.Name);

            IAspectsDictionary aspects = new AspectsDictionary();

            foreach (string k in htEntries.Keys)
            {
                aspects.Add(k, Convert.ToInt32(htEntries[k]));
            }

            entityProperty.SetValue(entityKeyedToPopulate, aspects);


            if (aspectsProp.KeyMustExistIn != null)
            {
                var mustExistInProperty =
                    entityProperties.SingleOrDefault(p => p.Name == aspectsProp.KeyMustExistIn);
                if (mustExistInProperty != null)
                {
                    foreach (var key in htEntries.Keys)
                    {
                        List<string> acceptableKeys =
                            mustExistInProperty.GetValue(entityKeyedToPopulate) as List<string>;

                        if (acceptableKeys == null)
                            _logger.LogProblem(
                                $"{entityKeyedToPopulate.GetType().Name} insists that {entityProperty.Name} should exist in {mustExistInProperty}, but that property is empty.");

                        if (!acceptableKeys.Contains(key))
                            _logger.LogProblem(
                                $"{entityKeyedToPopulate.GetType().Name} insists that {entityProperty.Name} should exist in {mustExistInProperty}, but the key {key} doesn't.");
                    }
                }
                else
                {
                    _logger.LogProblem(
                        $"{entityKeyedToPopulate.GetType().Name} insists that {entityProperty.Name} should exist in {aspectsProp.KeyMustExistIn}, but that property doesn't exist.");
                }
            }

        }

        public void PopulateWithDefaultValue(PropertyInfo entityProperty, dynamic entityToPopulate, Fucine attr)
        {

            if (Attribute.GetCustomAttribute(entityProperty, typeof(FucineId)) is FucineId)
            {

                _logger.LogProblem("ID not specified for a " + _entityType.Name);
            }
            else if (Attribute.GetCustomAttribute(entityProperty, typeof(FucineList)) is FucineList)
            {
                Type listType = entityProperty.PropertyType;
                entityProperty.SetValue(entityToPopulate, Activator.CreateInstance(listType));
            }
            else if (Attribute.GetCustomAttribute(entityProperty, typeof(FucineDict)) is FucineDict)
            {
                Type dictType = entityProperty.PropertyType;
                entityProperty.SetValue(entityToPopulate, Activator.CreateInstance(dictType));
            }


            else
            {
                entityProperty.SetValue(entityToPopulate, attr.DefaultValue);
            }
        }



    }
}
