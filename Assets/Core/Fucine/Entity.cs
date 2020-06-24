using System.Collections;
using System.Collections.Specialized;

namespace Assets.Core.Fucine
{
    public abstract class Entity
    {
        protected readonly Hashtable _unknownProperties = CollectionsUtil.CreateCaseInsensitiveHashtable();

        /// <summary>
        /// This is run for every top-level entity when the compendium has been completely (re)populated. Use for entities that
        /// need additional population based on data from other entities.
        /// It's not explicitly run for subentities - that's up to individual entities.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="populatedCompendium"></param>
        public virtual void RefineWithCompendium(ContentImportLogger logger, ICompendium populatedCompendium)
        {

        }

    public virtual void PushUnknownProperty(object key, object value)
        {
            _unknownProperties.Add(key, value);
        }

    public virtual Hashtable PopAllUnknownProperties()
     {
        Hashtable propertiesPopped = CollectionsUtil.CreateCaseInsensitiveHashtable(_unknownProperties);
        _unknownProperties.Clear();
        return propertiesPopped;
        }


    }
}
