using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;

namespace Assets.Core.Entities
{
    [FucineImportable("verbs")]
    public class BasicVerb: IVerb,IEntityWithId
    {
        private string _id;
        private readonly Hashtable _unknownProperties = CollectionsUtil.CreateCaseInsensitiveHashtable();

        [FucineId]
        public string Id
        {
            get => _id;
        }

        public void SetId(string id)
        {
            _id = id;
        }

        public void RefineWithCompendium(ContentImportLogger logger, ICompendium populatedCompendium)
        {
            Hashtable unknownProperties = PopAllUnknownProperties();
            if (unknownProperties.Keys.Count > 0)
            {
                foreach (var k in unknownProperties.Keys)
                    logger.LogInfo($"Unknown property in import: {k} for {GetType().Name} with ID {Id}");
            }
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

        [FucineValue(".")]
        public string Label { get; set; }

        [FucineValue(".")]
        public string Description { get; set; }

        [FucineSubEntity(typeof(SlotSpecification))]
        public SlotSpecification Slot { get; set; }


        public  bool Transient
        {
            get { return false; }
        }

        public BasicVerb(string id, string label, string description)
        {
            _id = id;
            Label = label;
            Description = description;
        }

        public BasicVerb()
        {

        }
    }
}
