using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Entities;
using Noon;
using OrbCreationExtensions;

namespace Assets.Core.Fucine
{
    public class FucinePropertyWalker<T> where T : new()
    {
        private readonly T _entityToPopulate;
        private readonly ContentImportLogger _logger;

        public FucinePropertyWalker(ContentImportLogger logger)
        {
            _logger = logger;
        }


        public T PopulateWith(Hashtable htEntityValues)
        {
            T entityToPopulate=new T();

            var entityProperties = typeof(T).GetProperties();

            foreach (var entityProperty in entityProperties)
            {
                try
                {
                    if (Attribute.GetCustomAttribute(entityProperty, typeof(FucineString)) is FucineString stringProp)
                    {
                        if (htEntityValues.ContainsKey(entityProperty.Name.ToLowerInvariant()))
                            entityProperty.SetValue(entityToPopulate, htEntityValues.GetValue(entityProperty.Name.ToLowerInvariant()));
                        else
                        {
                            if (stringProp.HasDefaultValue)
                                entityProperty.SetValue(entityToPopulate, stringProp.DefaultValue);
                            else
                            {
                                _logger.LogProblem("Couldn't find a mandatory property for a " +  typeof(T).Name + ": " + entityProperty.Name);
                            }
                        }
                    }

                    if (Attribute.GetCustomAttribute(entityProperty, typeof(FucineBool)) is FucineBool boolProp)
                    {
                        if (htEntityValues.ContainsKey(entityProperty.Name.ToLowerInvariant()))
                            entityProperty.SetValue(entityToPopulate, htEntityValues.GetBool(entityProperty.Name.ToLowerInvariant()));
                        else
                            entityProperty.SetValue(entityToPopulate, boolProp.DefaultValue);
                    }


                    if (Attribute.GetCustomAttribute(entityProperty, typeof(FucineInt)) is FucineInt intProp)
                    {
                        if (htEntityValues.ContainsKey(entityProperty.Name.ToLowerInvariant()))
                            entityProperty.SetValue(entityToPopulate, htEntityValues.GetInt(entityProperty.Name.ToLowerInvariant()));
                        else
                            entityProperty.SetValue(entityToPopulate, intProp.DefaultValue);
                    }


                    if (Attribute.GetCustomAttribute(entityProperty, typeof(FucineListString)) is FucineListString lsProp)
                    {
                        if (htEntityValues.ContainsKey(entityProperty.Name.ToLowerInvariant()))
                        {
                            ArrayList alStringList = htEntityValues.GetArrayList(entityProperty.Name.ToLowerInvariant());
                            List<string> stringList = new List<string>();
                            foreach (string s in alStringList)
                                stringList.Add(s);

                            entityProperty.SetValue(entityToPopulate, stringList);
                        }
                        else
                            _logger.LogProblem("Couldn't find a mandatory property for a " + typeof(T).Name + ": " + entityProperty.Name);
                    }

                    if (Attribute.GetCustomAttribute(entityProperty, typeof(FucineDictStringString)) is FucineDictStringString
                        dssProp)
                    {
                        if (htEntityValues.ContainsKey(entityProperty.Name.ToLowerInvariant()))
                        {
                            var htEntries = htEntityValues.GetHashtable(entityProperty.Name.ToLowerInvariant());
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
                                        List<string> acceptableKeys = mustExistInProperty.GetValue(entityToPopulate) as List<string>;

                                        if(acceptableKeys==null)
                                            _logger.LogProblem($"{entityToPopulate.GetType().Name} insists that {entityProperty.Name} should exist in {mustExistInProperty}, but that property is empty.");

                                        if (!acceptableKeys.Contains(key))
                                            _logger.LogProblem($"{entityToPopulate.GetType().Name} insists that {entityProperty.Name} should exist in {mustExistInProperty}, but the key {key} doesn't.");

                                    }

                                }
                                else
                                {
                                            _logger.LogProblem($"{entityToPopulate.GetType().Name} insists that {entityProperty.Name} should exist in {dssProp.KeyMustExistIn}, but that property doesn't exist.");

                                }

                            }

                            


                            //drawmessages property specifies a MustExist
                            //MustExist specifies spec

                            //foreach (var drawmessagekey in entityToPopulate.DrawMessages.Keys)
                            //{
                            //    if (!entityToPopulate.Spec.Contains(drawmessagekey))
                            //        _logger.LogProblem("Deckspec " + entityToPopulate.Id + " has a drawmessage for card " + drawmessagekey +
                            //                           ", but that card isn't in the list of drawable cards.");
                            //}
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogProblem("Problem importing property for a " + typeof(T).Name + ": "+ entityProperty.Name + ") - " + e.Message);
                }
            }

            return entityToPopulate;
        }



    }
}
