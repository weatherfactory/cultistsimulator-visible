using System.Collections.Generic;
using System.IO;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
    public static class LocalisationExtractor
    {
        [MenuItem("Tools/Build Localisation File")]
        public static void ValidateContentAssertions()
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
