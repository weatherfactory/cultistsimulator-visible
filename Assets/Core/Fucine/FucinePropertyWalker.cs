using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
        private readonly IEntityKeyed _entityKeyedToPopulate;
        private readonly ContentImportLogger _logger;
        private readonly Type _entityType;

        public FucinePropertyWalker(ContentImportLogger logger,Type entityType)
        {
            _logger = logger;
            _entityType = entityType;
        }


        public IEntity PopulateEntityWith(Hashtable importDataForEntity)
        {
           FucineEntityFactory factory=new FucineEntityFactory();
           IEntity newEntity = factory.CreateEntity(_entityType);
            
           var entityProperties = _entityType.GetProperties();

           FucinePopulator populator = new FucinePopulator(newEntity, importDataForEntity,_logger,_entityType);

            foreach (var thisProperty in entityProperties) 
            {
              
                //Walk every property. If the property is a Fucine property, 
                if (Attribute.GetCustomAttribute(thisProperty,typeof(Fucine)) is Fucine fucinePropertyAttribute)
                    //check if the import data has a key *at this level* for that property
                    if (importDataForEntity.ContainsKey(thisProperty.Name))
                        //if it does, populate the property (which may subsequently turn out to be an entity in its own right)
                       populator.PopulateProperty(importDataForEntity, thisProperty, newEntity, entityProperties);

                    else
                        //if there's no value for the property in the data, deal with it as unspecified
                        populator.PopulateWithDefaultValue(thisProperty, newEntity,
                            fucinePropertyAttribute);

            }

            return newEntity;
        }

       






    }
}
