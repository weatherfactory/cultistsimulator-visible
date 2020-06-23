using System.Collections;
using System.Collections.Specialized;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;

namespace Assets.Core.Entities
{
    public class Expulsion: IEntity
    {
        private Hashtable _unknownProperties = CollectionsUtil.CreateCaseInsensitiveHashtable();

        [FucineAspects]
        public AspectsDictionary Filter { get; set; }
        
        [FucineValue(1)] 
        public int Limit { get; set; }

        public Expulsion()
        {
            Filter = new AspectsDictionary();
        }

        public void RefineWithCompendium(ContentImportLogger logger, ICompendium populatedCompendium)
        {
            
        }

        public void PushUnknownProperty(object key, object value)
        {
            _unknownProperties.Add(key, value);
        }

        public Hashtable PopAllUnknownProperties()
        {
            Hashtable propertiesPopped = CollectionsUtil.CreateCaseInsensitiveHashtable(_unknownProperties);
            _unknownProperties.Clear();
            return propertiesPopped;
        }
    }
}