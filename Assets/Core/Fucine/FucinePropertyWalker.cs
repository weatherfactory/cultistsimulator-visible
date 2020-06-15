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


        public object PopulateEntityWith(Hashtable htEntityData)
        {
            dynamic entityToPopulate = Activator.CreateInstance(_entityType);

            var entityProperties = _entityType.GetProperties();

            foreach (var entityProperty in entityProperties)
            {

                if (htEntityData.ContainsKey(entityProperty.Name))
                    PopulateProperty(htEntityData, entityProperty, entityToPopulate, entityProperties);

                else
                   NotSpecifiedProperty(htEntityData, entityProperty, entityToPopulate, entityProperties);
            }

            return entityToPopulate;
        }

        private void PopulateProperty(Hashtable htEntityValues, PropertyInfo entityProperty, dynamic entityToPopulate,
            PropertyInfo[] entityProperties)
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

                else if (Attribute.GetCustomAttribute(entityProperty, typeof(FucineString)) is FucineString
                    stringProp)
                {
                    PopulateString(htEntityValues, entityProperty, entityToPopulate, stringProp);
                }

                else if (Attribute.GetCustomAttribute(entityProperty, typeof(FucineBool)) is FucineBool boolProp
                )
                {
                    PopulateBool(htEntityValues, entityProperty, entityToPopulate, boolProp);
                }


                else if (Attribute.GetCustomAttribute(entityProperty, typeof(FucineInt)) is FucineInt intProp)
                {
                    PopulateInt(htEntityValues, entityProperty, entityToPopulate, intProp);
                }

                else if (Attribute.GetCustomAttribute(entityProperty, typeof(FucineFloat)) is FucineFloat
                    floatProp)
                {
                    PopulateFloat(htEntityValues, entityProperty, entityToPopulate, floatProp);
                }

                else if (Attribute.GetCustomAttribute(entityProperty, typeof(FucineListGeneric)) is
                    FucineListGeneric lProp)
                {
                    PopulateListGeneric(htEntityValues, entityProperty, entityToPopulate, lProp);
                }


                else if (Attribute.GetCustomAttribute(entityProperty, typeof(FucineDictStringString)) is
                    FucineDictStringString
                    dssProp)
                {
                    PopulateDictStringString(htEntityValues, entityProperty, entityToPopulate, dssProp,
                        entityProperties);
                }


                else if (Attribute.GetCustomAttribute(entityProperty, typeof(FucineAspectsDictionary)) is
                    FucineAspectsDictionary
                    aspectsProp)
                {
                    PopulateAspectsDictionary(htEntityValues, entityProperty, entityToPopulate, aspectsProp,
                        entityProperties);
                }

                else if (Attribute.GetCustomAttribute(entityProperty, typeof(FucineEmanationProperty)) is
                    FucineEmanationProperty objectProp)
                {
                    PopulateEmanationProperty(htEntityValues, entityProperty, entityToPopulate, objectProp);
                }
            }
            catch (Exception e)
            {
                _logger.LogProblem("Problem importing property for a " + _entityType.Name + ": " +
                                   entityProperty.Name + ") - " + e.Message);
            }
        }


        private void PopulateString(Hashtable htEntityValues, PropertyInfo entityProperty, IEntity entityToPopulate,
            FucineString stringProp)
        {
                    entityProperty.SetValue(entityToPopulate, htEntityValues.GetString(entityProperty.Name));
        }

        private static void PopulateBool(Hashtable htEntityValues, PropertyInfo entityProperty, IEntity entityToPopulate,
            FucineBool boolProp)
        {
                entityProperty.SetValue(entityToPopulate, htEntityValues.GetBool(entityProperty.Name));
        }

        private static void PopulateInt(Hashtable htEntityValues, PropertyInfo entityProperty, IEntity entityToPopulate,
            FucineInt intProp)
        {
                entityProperty.SetValue(entityToPopulate, htEntityValues.GetInt(entityProperty.Name));
        }


        private void PopulateFloat(Hashtable htEntityValues, PropertyInfo entityProperty, object entityToPopulate, FucineFloat floatProp)
        {
                entityProperty.SetValue(entityToPopulate, htEntityValues.GetFloat(entityProperty.Name));
        }


        private void PopulateListGeneric(Hashtable htEntityValues, PropertyInfo entityProperty, object entityToPopulate, FucineListGeneric lProp)
        {

                ArrayList al = htEntityValues.GetArrayList(entityProperty.Name);
                Type listType = typeof(List<>);

                Type[] typeArgs = { lProp.MemberType };

                Type constructedType = listType.MakeGenericType(typeArgs);

              IList  list = Activator.CreateInstance(constructedType) as IList;
              foreach (var o in al)
              {
                  
                  if(o is Hashtable h) //if the arraylist contains hashtables
                  {
                      Hashtable cih = System.Collections.Specialized.CollectionsUtil.CreateCaseInsensitiveHashtable(h);
                      FucinePropertyWalker emanationWalker = new FucinePropertyWalker(_logger, lProp.MemberType);
                      var subEntity = emanationWalker.PopulateEntityWith(cih);
                      list.Add(subEntity);
                  }
                  else
                  {
                      list.Add(o); //This might not work for things that aren't strings?
                  }
                }

                entityProperty.SetValue(entityToPopulate, list);

        }

        private void PopulateEmanationProperty(Hashtable htEntityValues, PropertyInfo entityProperty, IEntity entityToPopulate, FucineEmanationProperty emanationProp)
        {

            string entityPropertyName = entityProperty.Name;
                FucinePropertyWalker emanationWalker = new FucinePropertyWalker(_logger, emanationProp.ObjectType);

                var subEntity = emanationWalker.PopulateEntityWith(htEntityValues.GetHashtable(entityPropertyName));

                entityProperty.SetValue(entityToPopulate, subEntity);

        }

        private void PopulateDictStringString(Hashtable htEntityValues, PropertyInfo entityProperty, IEntity entityToPopulate,
              FucineDictStringString dssProp, PropertyInfo[] entityProperties)
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

        private void PopulateAspectsDictionary(Hashtable htEntityValues, PropertyInfo entityProperty,
            IEntity entityToPopulate,
            FucineAspectsDictionary aspectsProp, PropertyInfo[] entityProperties)
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

        private void NotSpecifiedProperty(Hashtable htEntityValues, PropertyInfo entityProperty, dynamic entityToPopulate,
            PropertyInfo[] entityProperties)
        {
            try
            {
                if (Attribute.GetCustomAttribute(entityProperty, typeof(FucineId)) is FucineId)
                {

                    _logger.LogProblem("ID not specified for a " + _entityType.Name);
                }

                else
                {
                    FucineEntityProperty feProp=Attribute.GetCustomAttribute(entityProperty,typeof(FucineEntityProperty)) as FucineEntityProperty;
                    if(feProp!=null) // can we use LINQ or something to 
                        entityProperty.SetValue(entityToPopulate, feProp.DefaultValue);
                }

            }
            catch (Exception e)
            {
                _logger.LogProblem("Problem importing property for a " + _entityType.Name + ": " +
                                   entityProperty.Name + ") - " + e.Message);
            }
        }






    }
}
