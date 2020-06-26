using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;

namespace Assets.Core.Fucine
{
    public abstract class AbstractEntity<T> where T : AbstractEntity<T>
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
        public virtual void RefineWithCompendium(ContentImportLog log, ICompendium populatedCompendium)
        {
            if (Refined)
                return;
            Hashtable unknownProperties = PopAllUnknownProperties();

            if (unknownProperties.Keys.Count > 0)
            {
                foreach (var k in unknownProperties.Keys)
                    log.LogInfo($"Unknown property in import: {k} for {GetType().Name}");
            }

            Refined = true;
        }

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

        public HashSet<CachedFucineProperty<T>> GetFucinePropertiesCached()
        {
            return TypeInfoCache<T>.GetCachedFucinePropertiesForType();
        }

        protected AbstractEntity (Hashtable importDataForEntity,ContentImportLog log)
        {


            var fucineProperties = TypeInfoCache<T>.GetCachedFucinePropertiesForType();

            foreach (var cachedProperty in fucineProperties)
            {
                var importer = cachedProperty.GetImporterForProperty();
                bool imported = importer.TryImport<T>(this, cachedProperty, importDataForEntity,
                    typeof(T), log);
                if (imported)
                    importDataForEntity.Remove(cachedProperty.LowerCaseName);
            }


            foreach (var k in importDataForEntity.Keys)
            {
                PushUnknownProperty(k, importDataForEntity[k]);
            }

         
        }

        protected AbstractEntity()
        {

        }
    }

}
