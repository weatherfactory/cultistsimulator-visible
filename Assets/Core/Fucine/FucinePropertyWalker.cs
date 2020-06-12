using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Noon;
using OrbCreationExtensions;
using UnityEngine;

namespace Assets.Core.Fucine
{
    public class FucinePropertyWalker
    {
        private readonly IEntity _entityToPopulate;
        private readonly ContentImportLogger _logger;
        private readonly Type _entityType;

        public FucinePropertyWalker(ContentImportLogger logger,Type entityType)
        {
            _logger = logger;
            _entityType = entityType;
        }


        public object PopulateWith(Hashtable htEntityValues)
        {
            dynamic entityToPopulate = Activator.CreateInstance(_entityType);

            var entityProperties = _entityType.GetProperties();

            foreach (var entityProperty in entityProperties)
            {

                try
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

                    else if (Attribute.GetCustomAttribute(entityProperty, typeof(FucineString)) is FucineString stringProp)
                    {
                        PopulateString(htEntityValues, entityProperty, entityToPopulate, stringProp);
                    }

                    else if (Attribute.GetCustomAttribute(entityProperty, typeof(FucineBool)) is FucineBool boolProp)
                    {
                        PopulateBool(htEntityValues, entityProperty, entityToPopulate, boolProp);
                    }


                    else if(Attribute.GetCustomAttribute(entityProperty, typeof(FucineInt)) is FucineInt intProp)
                    {
                        PopulateInt(htEntityValues, entityProperty, entityToPopulate, intProp);
                    }

                    else if (Attribute.GetCustomAttribute(entityProperty, typeof(FucineFloat)) is FucineFloat floatProp)
                    {
                        PopulateFloat(htEntityValues, entityProperty, entityToPopulate, floatProp);
                    }

                    else if (Attribute.GetCustomAttribute(entityProperty, typeof(FucineList)) is FucineList lProp)
                    {
                        PopulateList(htEntityValues, entityProperty, entityToPopulate, lProp);
                    }


                    //else if (Attribute.GetCustomAttribute(entityProperty, typeof(FucineListString)) is FucineListString lsProp)
                    //{
                    //    PopulateListString(htEntityValues, entityProperty, entityToPopulate, lsProp);
                    //}

                    else if (Attribute.GetCustomAttribute(entityProperty, typeof(FucineDictStringString)) is
                        FucineDictStringString
                        dssProp)
                    {
                        PopulateDictStringString(htEntityValues, entityProperty, entityToPopulate, dssProp, entityProperties);
                    }


                    else if (Attribute.GetCustomAttribute(entityProperty, typeof(FucineAspectsDictionary)) is
                         FucineAspectsDictionary
                         aspectsProp)
                    {
                        PopulateAspectsDictionary(htEntityValues, entityProperty, entityToPopulate, aspectsProp, entityProperties);
                    }

                    else if (Attribute.GetCustomAttribute(entityProperty, typeof(FucineEmanationProperty)) is
                        FucineEmanationProperty objectProp)
                    {
                        PopulateEmanationProperty(htEntityValues, entityProperty, entityToPopulate, objectProp);
                    }

                }
                catch (Exception e)
                {
                    _logger.LogProblem("Problem importing property for a " + _entityType.Name + ": "+ entityProperty.Name + ") - " + e.Message);
                }
            }

            return entityToPopulate;
        }

        private void PopulateEmanationProperty(Hashtable htEntityValues, PropertyInfo entityProperty, IEntity entityToPopulate, FucineEmanationProperty emanationProp)
        {

            string entityPropertyName=entityProperty.Name;
            if (htEntityValues.ContainsKey(entityPropertyName))
            {
                FucinePropertyWalker emanationWalker=new FucinePropertyWalker(_logger,emanationProp.ObjectType);
                
                var subEntity = emanationWalker.PopulateWith(htEntityValues.GetHashtable(entityPropertyName));

                entityProperty.SetValue(entityToPopulate,subEntity);
            }
            else
            {
                _logger.LogProblem("Couldn't find a mandatory property (tho we may later allow non-mandatory options) for a " + _entityType.Name + " " +
                                   entityToPopulate.Id + ": " + entityProperty.Name);
            }

        }

        private void PopulateString(Hashtable htEntityValues, PropertyInfo entityProperty, IEntity entityToPopulate,
            FucineString stringProp)
        {
            if (htEntityValues.ContainsKey(entityProperty.Name))
                entityProperty.SetValue(entityToPopulate, htEntityValues.GetValue(entityProperty.Name));
            else
            {
                if (stringProp.HasDefaultValue)
                    entityProperty.SetValue(entityToPopulate, stringProp.DefaultValue);
                else
                {
                    _logger.LogProblem("Couldn't find a mandatory property for a " + _entityType.Name + " " +
                                       entityToPopulate.Id + ": " + entityProperty.Name);
                }
            }
        }

        private static void PopulateBool(Hashtable htEntityValues, PropertyInfo entityProperty, IEntity entityToPopulate,
            FucineBool boolProp)
        {
            if (htEntityValues.ContainsKey(entityProperty.Name))
                entityProperty.SetValue(entityToPopulate, htEntityValues.GetBool(entityProperty.Name));
            else
                entityProperty.SetValue(entityToPopulate, boolProp.DefaultValue);
        }

        private static void PopulateInt(Hashtable htEntityValues, PropertyInfo entityProperty, IEntity entityToPopulate,
            FucineInt intProp)
        {
            if (htEntityValues.ContainsKey(entityProperty.Name))
                entityProperty.SetValue(entityToPopulate, htEntityValues.GetInt(entityProperty.Name));
            else
                entityProperty.SetValue(entityToPopulate, intProp.DefaultValue);
        }


        private void PopulateFloat(Hashtable htEntityValues, PropertyInfo entityProperty, object entityToPopulate, FucineFloat floatProp)
        {
            if (htEntityValues.ContainsKey(entityProperty.Name))
                entityProperty.SetValue(entityToPopulate, htEntityValues.GetFloat(entityProperty.Name));
            else
                entityProperty.SetValue(entityToPopulate, floatProp.DefaultValue);
        }

        //private static void PopulateListString(Hashtable htEntityValues, PropertyInfo entityProperty, IEntity entityToPopulate,
        //    FucineListString lsProp)
        //{
        //    if (htEntityValues.ContainsKey(entityProperty.Name))
        //    {
        //        ArrayList alStringList = htEntityValues.GetArrayList(entityProperty.Name);
        //        List<string> stringList = new List<string>();
        //        foreach (string s in alStringList)
        //            stringList.Add(s);

        //        entityProperty.SetValue(entityToPopulate, stringList);
        //    }
        //    else
        //        entityProperty.SetValue(entityToPopulate, lsProp.DefaultValue);
        //}


        private void PopulateList(Hashtable htEntityValues, PropertyInfo entityProperty, object entityToPopulate, FucineList lProp)
        {
            if (htEntityValues.ContainsKey(entityProperty.Name))
            {
                ArrayList al = htEntityValues.GetArrayList(entityProperty.Name);
                Type listType = typeof(List<>);

                Type[] typeArgs = { lProp.MemberType };

                Type constructedType = listType.MakeGenericType(typeArgs);

              IList  list = Activator.CreateInstance(constructedType) as IList;
              foreach (var o in al)
              {
                  
                  if(o is Hashtable h)
                  {
                      Hashtable cih = System.Collections.Specialized.CollectionsUtil.CreateCaseInsensitiveHashtable(h);
                      FucinePropertyWalker emanationWalker = new FucinePropertyWalker(_logger, lProp.MemberType);
                      var subEntity = emanationWalker.PopulateWith(cih);
                      list.Add(subEntity);
                  }
                  else
                  {
                      list.Add(o); //This might not work for things that aren't strings?
                  }
                }

                entityProperty.SetValue(entityToPopulate, list);
            }
            else
                entityProperty.SetValue(entityToPopulate, lProp.DefaultValue);
        }


        private void PopulateDictStringString(Hashtable htEntityValues, PropertyInfo entityProperty, IEntity entityToPopulate,
            FucineDictStringString dssProp, PropertyInfo[] entityProperties)
        {
            if (htEntityValues.ContainsKey(entityProperty.Name))
            {
                var htEntries = htEntityValues.GetHashtable(entityProperty.Name);
                Dictionary<string, string> dictEntries =
                    NoonUtility.HashtableToStringStringDictionary(htEntries);
                entityProperty.SetValue(entityToPopulate, dictEntries);

                if (dssProp.KeyMustExistIn != null)
                {
                    var mustExistInProperty =
                        entityProperties.SingleOrDefault(p => p.Name == dssProp.KeyMustExistIn);
                    if (mustExistInProperty != null)
                    {
                        foreach (var key in dictEntries.Keys)
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
                            $"{entityToPopulate.GetType().Name} insists that {entityProperty.Name} should exist in {dssProp.KeyMustExistIn}, but that property doesn't exist.");
                    }
                }
            }
            else
            {
                entityProperty.SetValue(entityToPopulate, dssProp.DefaultValue);
            }
        }

        private void PopulateAspectsDictionary(Hashtable htEntityValues, PropertyInfo entityProperty, IEntity entityToPopulate,
            FucineAspectsDictionary aspectsProp, PropertyInfo[] entityProperties)
        {
            if (htEntityValues.ContainsKey(entityProperty.Name))
            {
                var htEntries = htEntityValues.GetHashtable(entityProperty.Name);

                IAspectsDictionary aspects = new AspectsDictionary();

                foreach (string k in htEntries.Keys)
                {
                    aspects.Add(k, Convert.ToInt32(htEntries[k]));
                }

                entityProperty.SetValue(entityToPopulate, aspects);


                if (aspectsProp.KeyMustExistIn != null)
                {
                    var mustExistInProperty =
                        entityProperties.SingleOrDefault(p => p.Name == aspectsProp.KeyMustExistIn);
                    if (mustExistInProperty != null)
                    {
                        foreach (var key in htEntries.Keys)
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
                            $"{entityToPopulate.GetType().Name} insists that {entityProperty.Name} should exist in {aspectsProp.KeyMustExistIn}, but that property doesn't exist.");
                    }
                }
            }
            else
            {
                entityProperty.SetValue(entityToPopulate, aspectsProp.DefaultValue);
            }
        }



    }
}
