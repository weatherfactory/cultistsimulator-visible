using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Interfaces;
using OrbCreationExtensions;

namespace Assets.Core.Fucine
{
   
    public abstract class FucineImport
    {
        protected ContentImportLogger _logger;
        protected PropertyInfo _property;

        protected FucineImport(PropertyInfo property, ContentImportLogger logger)
        {
            _property = property;
            _logger = logger;
        }

        public abstract void Populate(IEntity entity, Hashtable entityData,
            Type entityType);

        public static FucineImport CreateInstance(PropertyInfo property,ContentImportLogger logger,Hashtable entityData)
        {
                        if (entityData.ContainsKey(property.Name))
                        {
                            if (Attribute.GetCustomAttribute(property, typeof(FucineId)) is FucineId)
                                return new FucineImportId(property,logger);


                            else if (Attribute.GetCustomAttribute(property, typeof(FucineList)) is FucineList)
                                return new FucineImportList(property, logger);
            
                            else if (Attribute.GetCustomAttribute(property, typeof(FucineDict)) is FucineDict)
                                return new FucineImportDict(property, logger);

                            else if (Attribute.GetCustomAttribute(property, typeof(FucineAspects)) is FucineAspects)
                                return new FucineImportAspects(property, logger);

                            else if (Attribute.GetCustomAttribute(property, typeof(FucineSubEntity)) is FucineSubEntity)
                                return new FucineImportSubEntity(property, logger);

                            else if(Attribute.GetCustomAttribute(property, typeof(FucineValue)) is FucineValue)
                                return new FucineImportValue(property, logger);
                            else
                                throw new ApplicationException("Unknown Fucine property type on: " + property.Name);
                

                        }
                        else
                        {
                            return new FucineImportDefault(property, logger);
                        }
        }
    }

    public class FucineImportId : FucineImport
    {
        public FucineImportId(PropertyInfo property, ContentImportLogger logger) : base(property, logger)
        {

        }

        public override void Populate(IEntity entity, Hashtable entityData, Type entityType)
        {
            if (entityData.ContainsKey(_property.Name))
                (entity as IEntityUnique).SetId(entityData.GetValue(_property.Name) as string);

            else
            {
                _logger.LogProblem("ID not specified for a " + entityType.Name);
            }
        }
    }

    public class FucineImportValue : FucineImport
    {
        public FucineImportValue(PropertyInfo property, ContentImportLogger logger) : base(property, logger)
        {
        }

        public override void Populate(IEntity entity, Hashtable entityData, Type entityType)
        {
            TypeConverter typeConverter = TypeDescriptor.GetConverter(_property.PropertyType);

            _property.SetValue(entity, typeConverter.ConvertFromString(entityData[_property.Name].ToString()));
        }
    }



    public class FucineImportList : FucineImport
    {

        public FucineImportList(PropertyInfo property, ContentImportLogger logger) : base(property, logger)
        {
        }


        public override void Populate(IEntity entity, Hashtable entityData, Type entityType)
        {
            ArrayList al = entityData.GetArrayList(_property.Name);
            Type propertyListType = _property.PropertyType;
            Type listMemberType = propertyListType.GetGenericArguments()[0];


            IList list = Activator.CreateInstance(propertyListType) as IList;

            _property.SetValue(entity, list);

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
    }

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

    public class FucineImportAspects : FucineImport
    {
        public FucineImportAspects(PropertyInfo property, ContentImportLogger logger) : base(property, logger)
        {
        }

        public override void Populate(IEntity entity, Hashtable entityData, Type entityType)
        {
            var htEntries = entityData.GetHashtable(_property.Name);

            IAspectsDictionary aspects = new AspectsDictionary();

            var aspectsAttribute = Attribute.GetCustomAttribute(_property, typeof(FucineAspects)) as FucineAspects;
            var entityProperties = entityType.GetProperties();

            foreach (string k in htEntries.Keys)
            {
                aspects.Add(k, Convert.ToInt32(htEntries[k]));
            }

            _property.SetValue(entity, aspects);


            if (aspectsAttribute.KeyMustExistIn != null)
            {
                var mustExistInProperty =
                    entityProperties.SingleOrDefault(p => p.Name == aspectsAttribute.KeyMustExistIn);
                if (mustExistInProperty != null)
                {
                    foreach (var key in htEntries.Keys)
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
                        $"{entity.GetType().Name} insists that {_property.Name} should exist in {aspectsAttribute.KeyMustExistIn}, but that property doesn't exist.");
                }
            }
        }
    }

    public class FucineImportSubEntity : FucineImport
    {
        public FucineImportSubEntity(PropertyInfo property, ContentImportLogger logger) : base(property, logger)
        {
        }

        public override void Populate(IEntity entity, Hashtable entityData, Type entityType)
        {
            var subEntityAttribute = Attribute.GetCustomAttribute(_property, typeof(FucineSubEntity)) as FucineSubEntity;


            string entityPropertyName = _property.Name;
            FucinePropertyWalker emanationWalker = new FucinePropertyWalker(_logger, subEntityAttribute.ObjectType);

            var subEntity = emanationWalker.PopulateEntityWith(entityData.GetHashtable(entityPropertyName));

            _property.SetValue(entity, subEntity);
        }
    }

    public class FucineImportDefault : FucineImport
    {
        public FucineImportDefault(PropertyInfo property, ContentImportLogger logger) : base(property, logger)
        {
        }

        public override void Populate(IEntity entity, Hashtable entityData, Type entityType)
        {
            if (Attribute.GetCustomAttribute(_property, typeof(FucineId)) is FucineId )
            {

                _logger.LogProblem("ID not specified for a " + entityType.Name);
            }
            else if (Attribute.GetCustomAttribute(_property, typeof(FucineList)) is FucineList)
            {
                Type listType = _property.PropertyType;
                _property.SetValue(entity, Activator.CreateInstance(listType));
            }
            else if (Attribute.GetCustomAttribute(_property, typeof(FucineDict)) is FucineDict)
            {
                Type dictType = _property.PropertyType;
                _property.SetValue(entity, Activator.CreateInstance(dictType));
            }
            else if (Attribute.GetCustomAttribute(_property, typeof(FucineAspects)) is FucineAspects)
            {
                Type aspectsType = _property.PropertyType;
                _property.SetValue(entity, Activator.CreateInstance(aspectsType));
            }

            else if (Attribute.GetCustomAttribute(_property, typeof(FucineSubEntity)) is FucineSubEntity)
            {
                Type aspectsType = _property.PropertyType;
                _property.SetValue(entity, Activator.CreateInstance(aspectsType));
            }


            else if (Attribute.GetCustomAttribute(_property, typeof(FucineValue)) is FucineValue fucineValueAttr)
            {
                _property.SetValue(entity, fucineValueAttr.DefaultValue);
            }
            else
            {
                _logger.LogProblem($"Unknown or missing Fucine attribute type on {_property.Name} on {entityType.Name}");
            }
        }
    }


}
