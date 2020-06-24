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
using static System.String;

namespace Assets.Core.Fucine
{
    public class FucinePropertyWalker
    {
        private readonly IEntityWithId _entityWithIdToPopulate;
        private readonly ContentImportLogger _logger;
        private readonly Type _entityType;

        public FucinePropertyWalker(ContentImportLogger logger,Type entityType)
        {
            _logger = logger;
            _entityType = entityType;
        }


        public Entity PopulateEntityWith(Hashtable importDataForEntity)
        {
           FucineEntityFactory factory=new FucineEntityFactory();
           Entity newEntity = factory.CreateEntity(_entityType);
            
           var entityProperties = _entityType.GetProperties();

            foreach (var thisProperty in entityProperties)
            {
                if(Attribute.GetCustomAttributes(thisProperty,typeof(Fucine)).Any())
                {

                   FucineImport import = FucineImport.CreateInstance(thisProperty, _logger, importDataForEntity);
                   import.Populate(newEntity,importDataForEntity,_entityType);
                }
            }

            foreach (var k in importDataForEntity.Keys)
            {
             
                if(entityProperties.All(e => !string.Equals(e.Name, k.ToString(), StringComparison.InvariantCultureIgnoreCase)))
                {
                    newEntity.PushUnknownProperty(k,importDataForEntity[k]);

                }
            }

            return newEntity;
        }

       






    }
}
