using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
using OrbCreationExtensions;
using UnityEditor;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Assets.Editor
{
    public static class LocalisationTools
    {
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

        [MenuItem("Tools/Strip Localised Content")]
        public static void StripLocalisedContent()
        {
#if MODS
            new Registry().Register(new ModManager(false));
#endif
            var compendium = new Compendium();
            var contentImporter = new ContentImporter();
            contentImporter.PopulateCompendium(compendium);

            var pathToOldContentFiles = Path.Combine(Application.streamingAssetsPath, "content", "core_ru");
            var pathToNewContentFiles = pathToOldContentFiles + "_new";
            if (!Directory.Exists(pathToNewContentFiles))
            {
                Directory.CreateDirectory(pathToNewContentFiles);
            }
            var contentTypes = new[] {"decks", "elements", "endings", "legacies", "recipes", "verbs"};
            var validFields = new[] {"id", "label", "description", "startdescription", "drawmessages", "slots"};
            foreach (var contentType in contentTypes)
            {
                Debug.Log($"Processing {contentType}");
                var oldContentFolder = Path.Combine(pathToOldContentFiles, contentType);
                var newContentFolder = Path.Combine(pathToNewContentFiles, contentType);
                var oldContentFiles = Directory.GetFiles(oldContentFolder).Where(f => f.EndsWith(".json"));
                if (!Directory.Exists(newContentFolder))
                {
                    Directory.CreateDirectory(newContentFolder);
                }
                foreach (var oldContentFile in oldContentFiles)
                {
                    var fileName = Path.GetFileName(oldContentFile);
                    var newContentFileName = Path.Combine(newContentFolder, fileName);
                    Debug.Log($"\tProcessing {fileName}");

                    var table = SimpleJsonImporter.Import(File.ReadAllText(oldContentFile));
                    var items = table.GetArrayList(contentType);
                    foreach (Hashtable item in items)
                    {
                        var keys = new List<string>(item.Keys.Cast<string>());
                        foreach (var key in keys)
                        {
                            if (!validFields.Contains(key))
                            {
                                item.Remove(key);
                            }
                            else if (key == "slots")
                            {
                                var slotIdx = 0;
                                List<SlotSpecification> referenceSlots;
                                switch (contentType)
                                {
                                    case "elements":
                                        referenceSlots = compendium.GetElementById(item["id"].ToString())
                                            .ChildSlotSpecifications;
                                        break;
                                    case "recipes":
                                        referenceSlots = compendium.GetRecipeById(item["id"].ToString())
                                            .SlotSpecifications;
                                        break;
                                    case "verbs":
                                        referenceSlots = new List<SlotSpecification>
                                        {
                                            compendium.GetVerbById(item["id"].ToString()).PrimarySlotSpecification
                                        };
                                        break;
                                    default:
                                        throw new Exception($"Unexpected content type '{contentType}'");
                                }

                                foreach (Hashtable slotSpec in item.GetArrayList(key))
                                {
                                    var trueSlotId = referenceSlots[slotIdx++].Id;
                                    var slotLabel = slotSpec.GetString("label") ?? slotSpec.GetString("id");
                                    var slotDescription = slotSpec.GetString("description");
                                    slotSpec.Clear();
                                    slotSpec["id"] = trueSlotId;
                                    slotSpec["label"] = slotLabel;
                                    if (slotDescription != null)
                                    {
                                        slotSpec["description"] = slotDescription;
                                    }
                                }
                            }
                        }
                    }

                    var serializer = new SerializerBuilder()
                        .JsonCompatible()
                        .Build();
                    File.WriteAllText(newContentFileName, serializer.Serialize(table));
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
    }
}
