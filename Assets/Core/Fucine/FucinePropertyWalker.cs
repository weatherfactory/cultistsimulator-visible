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


        //public AbstractEntity<T> PopulateEntityWith(Hashtable importDataForEntity)
        //{
        //   FucineEntityFactory<T> factory =new FucineEntityFactory<T>();
        //   AbstractEntity<T> newEntity = factory.CreateEntity(_entityType);

   
        //  var fucineProperties = newEntity.GetFucinePropertiesCached();

        //    foreach (var fucineProperty in fucineProperties)
        //    {
        //        var importer = fucineProperty.FucineAttribute.CreateImporterInstance(fucineProperty, _log);
        //     bool imported=importer.TryImport(newEntity,importDataForEntity,_entityType);
        //     if(imported)
        //         importDataForEntity.Remove(fucineProperty.LowerCaseName);
        //    }

          
        //    foreach (var k in importDataForEntity.Keys)
        //    {
             
        //        newEntity.PushUnknownProperty(k,importDataForEntity[k]);

        //    }

        //    return newEntity;
        //}

       






    }
}
