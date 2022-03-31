using SecretHistories.Fucine.DataImport;
using Newtonsoft.Json.Linq;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SecretHistories.Constants;
using SecretHistories.UI;

namespace SecretHistories.Fucine

{
    public class EntityTypeDataLoader
    {
        public readonly Type EntityType;
        public readonly string EntityTag;
        public string BaseCulture { get; } = NoonConstants.DEFAULT_CULTURE_ID;
        public string CurrentCulture { get; set; }

        private readonly ContentImportLog _log;
        private List<LoadedDataFile> _coreContentFiles = new List<LoadedDataFile>();
        private List<LoadedDataFile> _modContentFiles = new List<LoadedDataFile>();
        private List<LoadedDataFile> _coreLocFiles = new List<LoadedDataFile>();
        private List<LoadedDataFile> _modLocFiles = new List<LoadedDataFile>();

        private Dictionary<string, EntityData> _allLoadedEntities { get; set; }

        /// <summary>
        /// k uniqueid, v original string from file
        /// </summary>
        private Dictionary<string, string> _localisedTextValuesRegistry { get; set; }

        public EntityTypeDataLoader(Type entityType, string entityTag, string currentCulture, ContentImportLog log)
        {
            EntityType = entityType;
            EntityTag = entityTag;
            _log = log;
            this.CurrentCulture = currentCulture;
            _allLoadedEntities = new Dictionary<string, EntityData>();
            _localisedTextValuesRegistry = new Dictionary<string, string>();
        }

        /// <summary>
        /// This can handle being called more than once, although it won't check uniqueness
        /// </summary>
        public void SupplyContentFiles(IEnumerable<LoadedDataFile> coreContentFiles,
            IEnumerable<LoadedDataFile> locContentFiles, IEnumerable<LoadedDataFile> modContentFiles, IEnumerable<LoadedDataFile> modLocFiles)
        {
            _coreContentFiles.AddRange(coreContentFiles);
            _coreLocFiles.AddRange(locContentFiles);
            _modContentFiles.AddRange(modContentFiles);
            _modLocFiles.AddRange(modLocFiles);
        }

        public void LoadEntityDataFromSuppliedFiles()
        {
            //load localised data if we're using a non-default culture.
            //We'll use the unique field ids to replace data with localised data down in UnpackToken, if we find matching ids
            //String $ operations - "$prefix", "$postfix", "$replace" are localized
            if (BaseCulture != CurrentCulture)
            {
                var localisableKeysForEntityType = GetLocalisableKeysForEntityType();

                if (_coreLocFiles.Any())
                    RegisterLocalisedDataForEmendations(_coreLocFiles, localisableKeysForEntityType);

                if (_modLocFiles.Any())
                    RegisterLocalisedDataForEmendations(_modLocFiles, localisableKeysForEntityType);
            }

            //core content and mod content are loaded and handled uniformly - as a list, which will later be ordered by $priority and converted into the final content dictionary
            //this allows core content to make use of $ operations (among which are JSON-level inheritance, which'll surely come in handy)
            //also allows mods to prioritise their content over the main game (currently important only for craftable recipes evaluation order, but who knows what the future'll bring)
            //(so, core content is effectively just another "mod" that's always on top of the loading order)
            List<EntityData> allEntitiesInFiles = new List<EntityData>();
            allEntitiesInFiles.AddRange(UnpackAndLocaliseContent(_coreContentFiles));
            allEntitiesInFiles.AddRange(UnpackAndLocaliseContent(_modContentFiles));

            string[] ignoredContentGroups = Watchman.Get<Config>().GetConfigValueAsArray(NoonConstants.IGNOREDCONTENT);

            IOrderedEnumerable<EntityData> allEntitiesOrdered = allEntitiesInFiles
                .Where(entityData => entityData.GetAndFlushIgnore(ignoredContentGroups) == false) //entities that are inside ignored content groups are excluded
                .OrderByDescending(entityData => entityData.GetAndFlushPriority(_log));
            //all non-excluded entities are sorted by their $priority
            //if entity has no defined priority, it's defaulted as 0
            //entities with the same priority are ordered as they were: (core) > (mods), (file order) > (definitions in file from top to bottom)
            //thus, the previous loading order is preserved for everything that makes no use of this operation
            //(there's still an imperfection that you can't put anything in-between the things with the same priority)

            _allLoadedEntities = new Dictionary<string, EntityData>(); //it gets created in a constructor, but we need to clear it anyway and just in case
            //we're then applying all loaded entities to the final dictionary - one by one, so nothing gets lost
            foreach (EntityData entityData in allEntitiesOrdered)
                entityData.ApplyDataToCollection(_allLoadedEntities, _log);
        }

        private HashSet<string> GetLocalisableKeysForEntityType()
        {
            var localizableKeys = new HashSet<string>();

            var entityTypeProperties = EntityType.GetProperties();

            foreach (var thisProperty in entityTypeProperties)
            {
                //Note: this doesn't work quite in the way I intended it. Label and Description on Slots are marked as Localise, but
                //the attribute is inspected only for the top level entity. Because the top level entity also has Label and Description marked as localise,
                //the slot properties are added to the localisable keys, but this will break if the names are different. Consider explicitly inspecting subproperty attributes
                //to see if they're also subentities, when loading the data
                if (Attribute.GetCustomAttribute(thisProperty, typeof(Fucine)) is Fucine fucineAttribute)
                {
                    if (fucineAttribute.Localise)
                    {
                        string propertyName = thisProperty.Name.ToLower();
                        localizableKeys.Add(propertyName);
                        localizableKeys.Add(propertyName + "$prefix");
                        localizableKeys.Add(propertyName + "$postfix");
                        localizableKeys.Add(propertyName + "$replace");

                        //need to add these too, because they can contain prefixes/postfixes/replaces
                        localizableKeys.Add(propertyName + "$dictedit");
                        localizableKeys.Add(propertyName + "$listedit");
                    }
                }
            }

            return localizableKeys;
        }

        public List<EntityData> GetLoadedEntityDataAsList()
        {
            return _allLoadedEntities.Values.ToList();
        }

        private List<EntityData> UnpackAndLocaliseContent(List<LoadedDataFile> contentFiles)
        {
            List<EntityData> allModEntities = new List<EntityData>();
            foreach (LoadedDataFile modContentFile in contentFiles)
            {
                FucineUniqueIdBuilder containerBuilder = new FucineUniqueIdBuilder(modContentFile.EntityContainer);
                JArray topLevelArrayList = (JArray)modContentFile.EntityContainer.Value;

                foreach (JToken eachObject in topLevelArrayList)
                {
                    EntityData entityData = UnpackAndLocaliseEntityData(eachObject, containerBuilder);
                    allModEntities.Add(entityData);
                }
            }

            return allModEntities;
        }

        private EntityData UnpackAndLocaliseEntityData(JToken eachObject, FucineUniqueIdBuilder containerBuilder)
        {
            var eachObjectHashtable = new Hashtable(); //eg Work verb
            var entityBuilder = new FucineUniqueIdBuilder(eachObject, containerBuilder);

            foreach (var eachProperty in ((JObject)eachObject).Properties()
                    ) //eg description, but also eg slots - this is why we need to unpack
            {
                eachObjectHashtable.Add(eachProperty.Name.ToLower(),
                    UnpackAndLocaliseToken(eachProperty, entityBuilder));
            }

            return new EntityData(entityBuilder.UniqueId, eachObjectHashtable);
        }

        /// <summary>
        /// Unpack the tokens into individual entity data all the way round, and also replace data that's been localised or modded.
        /// </summary>
        /// <returns></returns>
        private object UnpackAndLocaliseToken(JToken jToken, FucineUniqueIdBuilder tokenIdBuilder)
        {
            if (jToken.Type == JTokenType.Property)
            {
                var propertyBuilder = new FucineUniqueIdBuilder(jToken, tokenIdBuilder);
                return UnpackAndLocaliseToken(((JProperty)jToken).Value, propertyBuilder);
            }

            if (jToken.Type == JTokenType.Array)
            {
                var nextList = new ArrayList();
                foreach (var eachItem in (JArray)jToken)
                {
                    var nextBuilder = new FucineUniqueIdBuilder(jToken, tokenIdBuilder);
                    nextList.Add(UnpackAndLocaliseToken(eachItem, nextBuilder));
                }

                return nextList;
            }

            else if (jToken.Type == JTokenType.Object)
            {

                //create a hashtable to represent the object
                var subObjectH = new Hashtable();

                var subObjectBuilder = new FucineUniqueIdBuilder(jToken, tokenIdBuilder);


                foreach (var subProperty in ((JObject)jToken).Properties())
                {
                    var subPropertyIdBuilder = new FucineUniqueIdBuilder(subProperty, subObjectBuilder);
                    //add each property to that hashtable
                    subObjectH.Add(subProperty.Name.ToLower(), UnpackAndLocaliseToken(subProperty.Value, subPropertyIdBuilder));
                }


                EntityData subEntityData = new EntityData(subObjectBuilder.UniqueId, subObjectH);
                //return the entityData so it can be added in its turn, with the unpacked object
                return subEntityData;
            }

            else
            {
                if (jToken.Type == JTokenType.String)
                {


                    string uniqueTokenId = new FucineUniqueIdBuilder(jToken, tokenIdBuilder).UniqueId;
                    if (CurrentCulture != BaseCulture)
                        return TryReplaceWithLocalisedString(jToken, uniqueTokenId);
                    else
                        return jToken.ToString();

                }

                else if (jToken.Type == JTokenType.Integer)
                {
                    return (int)jToken;
                }


                else if (jToken.Type == JTokenType.Boolean)
                {
                    return (bool)jToken;
                }

                else if (jToken.Type == JTokenType.Float)
                {
                    return (double)jToken;
                }
                else
                {
                    throw new ApplicationException("Unexpected jtoken type: " + jToken.Type);
                }
            }
        }

        private object TryReplaceWithLocalisedString(JToken jToken, string uniqueTokenId)
        {
            if (_localisedTextValuesRegistry.TryGetValue(uniqueTokenId, out var localisedString))
                return localisedString;
            else
                return jToken.ToString();
        }

        private void RegisterLocalisedDataForEmendations(List<LoadedDataFile> locFilesToProcess, HashSet<string> localizableKeys)
        {
            foreach (var locContentFile in locFilesToProcess)
            {
                var containerBuilder = new FucineUniqueIdBuilder(locContentFile.EntityContainer);

                var topLevelArrayList = (JArray)locContentFile.EntityContainer.Value;


                foreach (var eachObject in topLevelArrayList)
                {
                    var entityBuilder = new FucineUniqueIdBuilder(eachObject, containerBuilder);

                    foreach (var eachProperty in ((JObject)eachObject).Properties())
                    {

                        //There's another bug here that approximately cancels out the bug described above when scanning for localisable keys.
                        //We only check the top-level property, not its sub-properties. So if we're registering loc data for 'linked' (localisable=true)
                        //we also register loc data for all its sub properties - eg both 'startdescription' (hurray) and ID (minor perf pain)

                        if (localizableKeys.Contains(eachProperty.Name))
                        {
                            var propertyIdBuilder = new FucineUniqueIdBuilder(eachProperty, entityBuilder);
                            RegisterEmendationValues(eachProperty.Value, propertyIdBuilder, _localisedTextValuesRegistry);
                        }
                    }
                }
            }
        }

        private void RegisterEmendationValues(JToken jtoken, FucineUniqueIdBuilder nameBuilder, Dictionary<string, string> valuesRegistry)
        {
            if (jtoken.Type == JTokenType.Object)
            {
                FucineUniqueIdBuilder subObjectBuilder = new FucineUniqueIdBuilder(jtoken, nameBuilder);

                foreach (JProperty jProperty in ((JObject)jtoken).Properties())
                {
                    var subPropertyBuilder = new FucineUniqueIdBuilder(jProperty, subObjectBuilder);
                    RegisterEmendationValues(jProperty.Value, subPropertyBuilder, valuesRegistry);
                }
            }

            else if (jtoken.Type == JTokenType.Array)
            {
                FucineUniqueIdBuilder arrayBuilder = new FucineUniqueIdBuilder(jtoken, nameBuilder);

                foreach (var item in ((JArray)jtoken))
                {
                    RegisterEmendationValues(item, arrayBuilder, valuesRegistry);
                }
            }

            else if (jtoken.Type == JTokenType.String)
            {
                FucineUniqueIdBuilder builder = new FucineUniqueIdBuilder(jtoken, nameBuilder);
                if (valuesRegistry.ContainsKey(builder.UniqueId))
                    valuesRegistry[builder.UniqueId] = (string)jtoken;
                else
                    valuesRegistry.Add(builder.UniqueId, (string)jtoken);
            }

            else

            {
                //Probably superfluous non-translated json kept from the original.
            }
        }






    }
}