using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;

namespace Assets.Core.Fucine
{
    public interface IEntity
    {
    void OnPostImport(ContentImportLog log, ICompendium populatedCompendium);
    }

    public abstract class AbstractEntity<T>: IEntity where T : AbstractEntity<T>
    {
        protected bool Refined = false;
        protected readonly Hashtable UnknownProperties = new Hashtable();

        /// <summary>
        /// This is run for every top-level entity when the compendium has been completely (re)populated. Use for entities that
        /// need additional population based on data from other entities.
        /// It's not explicitly run for subentities - that's up to individual entities.
        /// Overriding implementations should set refined to true, and not run it if it isn't - this isn't yet enforced
        /// </summary>
        /// <param name="log"></param>
        /// <param name="populatedCompendium"></param>
        public virtual void OnPostImport(ContentImportLog log, ICompendium populatedCompendium)
        {
            if (Refined)
                return;

            var fucineProperties = TypeInfoCache<T>.GetCachedFucinePropertiesForType();

            foreach (var cachedProperty in fucineProperties)
            {
                if (cachedProperty.FucineAttribute.ValidateAsElementId)
                {
                    object toValidate = cachedProperty.GetViaFastInvoke(this as T);
                    populatedCompendium.AddElementIdsToValidate(toValidate);

                }
            }


            OnPostImportEntitySpecifics(log,populatedCompendium);


            Hashtable unknownProperties = PopAllUnknownProperties();

            if (unknownProperties.Keys.Count > 0)
            {
                foreach (var k in unknownProperties.Keys)
                    log.LogInfo($"Unknown property in import: {k} for {GetType().Name}");
            }

            Refined = true;
        }

        protected abstract void OnPostImportEntitySpecifics(ContentImportLog log, ICompendium populatedCompendium);

        public void PushUnknownProperty(object key, object value)
        {
            UnknownProperties.Add(key, value);
        }

        public virtual Hashtable PopAllUnknownProperties()
        {
            Hashtable propertiesPopped = new Hashtable(UnknownProperties);
            UnknownProperties.Clear();
            return propertiesPopped;
        }


        protected AbstractEntity (Hashtable importDataForEntity,ContentImportLog log)
        {
            try
            {
                var fucineProperties = TypeInfoCache<T>.GetCachedFucinePropertiesForType();

                foreach (var cachedProperty in fucineProperties)
                {
                    var importer = cachedProperty.GetImporterForProperty();
                    bool imported = importer.TryImportProperty<T>(this as T, cachedProperty, importDataForEntity, log);
                    if (imported)
                        importDataForEntity.Remove(cachedProperty.LowerCaseName);
                }


                foreach (var k in importDataForEntity.Keys)
                {
                    PushUnknownProperty(k, importDataForEntity[k]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        protected AbstractEntity()
        {

        }
    }

}
