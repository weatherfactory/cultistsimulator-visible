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
                AbstractFucineImporter importer;

                if (fucineProperty.FucineAttribute is FucineId)
                    importer= new IdImporter(fucineProperty, _log);
                //Try whether the key exists or not: we might be using an arbitrary internal key

                else if (importDataForEntity.ContainsKey(fucineProperty.Name))
                {
                    importer=fucineProperty.FucineAttribute.CreateImporterInstance(fucineProperty, _log);
                }
                else
                {
                    importer =new FucineImportDefault(fucineProperty, _log);
                }

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
