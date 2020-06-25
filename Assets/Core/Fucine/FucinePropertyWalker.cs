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
        private readonly ContentImportLog _log;
        private readonly Type _entityType;

        public FucinePropertyWalker(ContentImportLog log,Type entityType)
        {
            _log = log;
            _entityType = entityType;
        }


        public AbstractEntity PopulateEntityWith(Hashtable importDataForEntity)
        {
           FucineEntityFactory factory=new FucineEntityFactory();
           AbstractEntity newEntity = factory.CreateEntity(_entityType);

            
          var fucineProperties = newEntity.GetFucinePropertiesCached();

            foreach (var fucineProperty in fucineProperties)
            {
                AbstractImporter importer;

                importer = fucineProperty.FucineAttribute.CreateImporterInstance(fucineProperty, _log);
                importer.Populate(newEntity,importDataForEntity,_entityType);
            }

            
            foreach (var k in importDataForEntity.Keys)
            {
             
                if(fucineProperties.All(e => !string.Equals(e.PropertyInfo.Name, k.ToString(), StringComparison.InvariantCultureIgnoreCase)))
                {
                    newEntity.PushUnknownProperty(k,importDataForEntity[k]);

                }
            }

            return newEntity;
        }

       






    }
}
