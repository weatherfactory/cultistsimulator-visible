using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Noon;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
    public static class LocalisationTools
    {
        private static readonly string[] ContentTypes =
        {
            "decks", "elements", "endings", "legacies", "recipes", "verbs"
        };
        private static readonly string[] ValidFields =
        {
            "id",
            "label",
            "description",
            "startdescription",
            "drawmessages",
            "slots",
            "linked",
            "alt",
            "alternativerecipes"
        };

        [MenuItem("Tools/Build Localisation File")]
        public static void BuildLocalisationFile()
        {
            var path = EditorUtility.SaveFilePanel(
                "Save translatable lines to file",
                "",
                "cs_loc_lines.txt",
                "txt");

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            Debug.Log("Generating localisation file");

#if MODS
            new Registry().Register(new ModManager(false));
#endif
            var compendium = new Compendium();
            var contentImporter = new ContentImporter();
            contentImporter.PopulateCompendium(compendium);
            using (var file = new StreamWriter(path, false))
            {
                // Decks
                foreach (var deck in compendium.GetAllDeckSpecs())
                {
                    Write(file, deck.Label);
                    Write(file, deck.Description);
                    Write(file, deck.DrawMessages?.Values);
                    Write(file, deck.DefaultDrawMessages?.Values);
                }

                // Elements
                foreach (var element in compendium.GetAllElementsAsDictionary().Values)
                {
                    Write(file, element.Label);
                    Write(file, element.Description);
                }

                // Endings
                foreach (var ending in compendium.GetAllEndings())
                {
                    Write(file, ending.Title);
                    Write(file, ending.Description);
                }

                // Recipes
                foreach (var recipe in compendium.GetAllRecipesAsList())
                {
                    Write(file, recipe.Label);
                    Write(file, recipe.StartDescription);
                    Write(file, recipe.Description);
                }
            }

            Debug.Log($"Localisation file saved at '{path}'");
        }

        [MenuItem("Tools/Update Localisation Stubs")]
        public static void AddLocalisationStubs()
        {
            var sourceContentPath = Path.Combine(Application.streamingAssetsPath, "content", "core");
            var destContentPath = Path.Combine(Application.streamingAssetsPath, "content", "core_ru");
            if (!Directory.Exists(destContentPath))
            {
                Directory.CreateDirectory(destContentPath);
            }

            foreach (var contentType in ContentTypes)
            {
                var sourcePath = Path.Combine(sourceContentPath, contentType);
                var sourceFiles = Directory.GetFiles(sourcePath).Where(f => f.EndsWith(".json"));
                var destPath = Path.Combine(destContentPath, contentType);
                if (!Directory.Exists(destPath))
                {
                    Directory.CreateDirectory(destPath);
                }

                foreach (var sourceFile in sourceFiles)
                {
                    var fileName = Path.GetFileName(sourceFile);
                    NoonUtility.Log($"Processing {contentType}/{fileName}");
                    var destFile = Path.Combine(destPath, fileName);

                    var data = SimpleJsonImporter.Import(File.ReadAllText(sourceFile));

                    // Copy any existing translations first
                    if (File.Exists(destFile))
                    {
                        var translatedEntities =
                            ((ArrayList) SimpleJsonImporter.Import(File.ReadAllText(destFile))[contentType])
                            .Cast<Hashtable>()
                            .ToDictionary(entity => (string) entity["id"]);
                        foreach (Hashtable entity in (ArrayList) data[contentType])
                        {
                            var entityId = (string) entity["id"];
                            if (!translatedEntities.ContainsKey(entityId))
                            {
                                continue;
                            }

                            var translatedEntity = translatedEntities[entityId];
                            CopyTranslatedFields(entity, translatedEntity);
                        }
                    }

                    data[contentType] = PruneArrayList((ArrayList) data[contentType]);
                    WriteJsonToFile(destFile, data);
                }
            }
        }

        private static void CopyTranslatedFields(Hashtable original, Hashtable translated)
        {
            foreach (var property in translated.Keys.Cast<string>().ToList())
            {
                switch (property)
                {
                    case "label":
                    case "description":
                    case "startdescription":
                        original[property] = translated[property];
                        break;

                    case "drawmessages":
                        var drawMessages = (Hashtable) original[property];
                        var translatedDrawMessages = (Hashtable) translated[property];
                        foreach (var drawMessageId in drawMessages.Keys.Cast<string>().ToList()
                            .Where(drawMessageId => translatedDrawMessages.ContainsKey(drawMessageId)))
                        {
                            drawMessages[drawMessageId] = translatedDrawMessages[drawMessageId];
                        }
                        break;

                    case "slots":
                    case "linked":
                    case "alt":
                    case "alternativerecipes":
                        var subEntities = (ArrayList) original[property];
                        var translatedSubEntities = (ArrayList) translated[property];
                        for (var i = 0; i < subEntities.Count; i++)
                        {
                            var subEntity = (Hashtable) subEntities[i];
                            var translatedSubEntity = (Hashtable) translatedSubEntities[i];
                            CopyTranslatedFields(subEntity, translatedSubEntity);
                        }
                        break;
                }
            }
        }

        private static ArrayList PruneArrayList(IEnumerable source)
        {
            return new ArrayList(source.Cast<Hashtable>().Select(PruneHashtable).ToList());
        }

        private static Entity PruneHashtable(Hashtable source)
        {
            var newEntity = new Entity();
            foreach (string property in source.Keys.Cast<string>().Where(p => ValidFields.Contains(p)).ToList())
            {
                switch (property)
                {
                    case "slots":
                    case "linked":
                    case "alt":
                    case "alternativerecipes":
                        var newList = PruneArrayList((ArrayList) source[property]);
                        var onlyIds = newList.Cast<Entity>().All(entry => entry.IsIdOnly());

                        if (!onlyIds)
                        {
                            SetEntityProperty(newEntity, property, newList);
                        }
                        break;
                    default:
                        SetEntityProperty(newEntity, property, source[property]);
                        break;
                }
            }
            return newEntity;
        }

        private static void SetEntityProperty(Entity entity, string name, object value)
        {
            typeof(Entity).GetProperty(name).SetValue(entity, value);
        }

        private static void WriteJsonToFile(string filePath, object data)
        {
            using (var file = File.OpenWrite(filePath))
            {
                using (var writer = new StreamWriter(file))
                {
                    using (var jsonWriter = new JsonTextWriter(writer))
                    {
                        jsonWriter.Formatting = Formatting.Indented;
                        jsonWriter.Indentation = ' ';
                        jsonWriter.Indentation = 4;
                        JsonSerializer serializer = new JsonSerializer {NullValueHandling = NullValueHandling.Ignore};
                        serializer.Serialize(jsonWriter, data);
                    }
                }
            }
        }

        private static void Write(TextWriter writer, string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                writer.WriteLine(text.Replace("\n", @"\n"));
            }
        }

        private static void Write(TextWriter writer, IEnumerable<string> texts)
        {
            if (texts != null)
            {
                foreach (var text in texts)
                {
                    Write(writer, text);
                }
            }
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private class Entity
        {
            [JsonProperty(Order = 1)]
            public string id { get; set; }

            [JsonProperty(Order = 2)]
            public string label { get; set; }

            [JsonProperty(Order = 3)]
            public string description { get; set; }

            [JsonProperty(Order = 4)]
            public string startdescription { get; set; }

            [JsonProperty(Order = 5)]
            public Hashtable drawmessages { get; set; }

            [JsonProperty(Order = 6)]
            public ArrayList slots { get; set; }

            [JsonProperty(Order = 7)]
            public ArrayList linked { get; set; }

            [JsonProperty(Order = 8)]
            public ArrayList alt { get; set; }

            [JsonProperty(Order = 9)]
            public ArrayList alternativerecipes { get; set; }

            public bool IsIdOnly()
            {
                return
                    label == null
                    && description == null
                    && startdescription == null
                    && drawmessages == null
                    && slots == null
                    && linked == null
                    && alt == null
                    && alternativerecipes == null;
            }
        }
    }
}
