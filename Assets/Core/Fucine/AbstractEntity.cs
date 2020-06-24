using System.Collections;
using System.Collections.Specialized;

namespace Assets.Core.Fucine
{
    public abstract class AbstractEntity
    {
        protected bool Refined = false;
        protected readonly Hashtable UnknownProperties = CollectionsUtil.CreateCaseInsensitiveHashtable();

        /// <summary>
        /// This is run for every top-level entity when the compendium has been completely (re)populated. Use for entities that
        /// need additional population based on data from other entities.
        /// It's not explicitly run for subentities - that's up to individual entities.
        /// Overriding implementations should set refined to true, and not run it if it isn't - this isn't yet enforced
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="populatedCompendium"></param>
        public virtual void RefineWithCompendium(ContentImportLogger logger, ICompendium populatedCompendium)
        {
            if (Refined)
                return;
            Hashtable unknownProperties = PopAllUnknownProperties();

            if (unknownProperties.Keys.Count > 0)
            {
                foreach (var k in unknownProperties.Keys)
                    logger.LogInfo($"Unknown property in import: {k} for {GetType().Name}");
            }

            Refined = true;
        }

    public virtual void PushUnknownProperty(object key, object value)
        {
            UnknownProperties.Add(key, value);
        }

    public virtual Hashtable PopAllUnknownProperties()
     {
        Hashtable propertiesPopped = CollectionsUtil.CreateCaseInsensitiveHashtable(UnknownProperties);
        UnknownProperties.Clear();
        return propertiesPopped;
        }


    }
}
