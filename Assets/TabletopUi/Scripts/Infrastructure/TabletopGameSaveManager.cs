using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using OrbCreationExtensions;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public class TabletopGameSaveManager
    {
        private ITabletopGameExporter exporter;
        private ICompendium compendium;

        public TabletopGameSaveManager(ITabletopGameExporter exporter,ICompendium compendium)
        {
            this.exporter = exporter;
            this.compendium = compendium;
        }

        public void SaveGame(TabletopContainer tabletopContainer,string saveFileName)
        {
            var htSaveTable = exporter.Export(tabletopContainer.GetElementStacksManager().GetStacks(), tabletopContainer.GetAllSituationTokens());
            File.WriteAllText(Noon.NoonUtility.GetGameSavePath(saveFileName), htSaveTable.JsonString());
        }

        public Hashtable LoadSavedGame(string saveFileName)
        {
            string importJson = File.ReadAllText(Noon.NoonUtility.GetGameSavePath(saveFileName));
            Hashtable htSave = SimpleJsonImporter.Import(importJson);
            return htSave;
        }


        public void ImportSavedGameToContainer(TabletopContainer tabletopContainer, Hashtable htSave)
        {
            var htElementStacks = htSave.GetHashtable(Noon.NoonConstants.CONST_SAVE_ELEMENTSTACKS);
            var htSituations = htSave.GetHashtable(Noon.NoonConstants.CONST_SAVE_SITUATIONS);

            foreach (var locatorId in htElementStacks.Keys)
            {
                var htElement =
                    Noon.NoonUtility.HashtableToStringIntDictionary(htElementStacks.GetHashtable(locatorId));

                tabletopContainer.GetElementStacksManager()
                    .IncreaseElement(htElement.Keys.Single(), htElement.Values.Single(), locatorId.ToString());
            }


            foreach (var locatorId in htSituations.Keys)
            {
                var htSituation =
                    Noon.NoonUtility.HashtableToStringStringDictionary(htSituations.GetHashtable(locatorId));

                IVerb situationVerb = compendium.GetVerbById(htSituation.Keys.Single());
                string recipeId = htSituation.Values.Single();
                tabletopContainer.CreateSituation(situationVerb,recipeId,locatorId.ToString());
                
            }
    }
    }
}
