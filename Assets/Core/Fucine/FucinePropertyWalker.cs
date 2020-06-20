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
        private readonly IEntityWithId _entityWithIdToPopulate;
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

            foreach (var thisProperty in entityProperties)
            {
                if(Attribute.GetCustomAttributes(thisProperty,typeof(Fucine)).Any())
                {

                   FucineImport import = FucineImport.CreateInstance(thisProperty, _logger, importDataForEntity);
                    import.Populate(newEntity,importDataForEntity,_entityType);
                }
            }

            return newEntity;
        }

       






    }
}
