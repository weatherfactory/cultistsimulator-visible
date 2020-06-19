using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Core.Interfaces;
using OrbCreationExtensions;

namespace Assets.Core.Fucine
{
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
}