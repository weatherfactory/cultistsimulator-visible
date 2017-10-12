using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using Noon;
using OrbCreationExtensions;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public class GameSaveManager
    {
        private readonly IGameDataImporter dataImporter;
        private readonly IGameDataExporter dataExporter;

        
        public GameSaveManager(IGameDataImporter dataImporter,IGameDataExporter dataExporter)
        {
            this.dataImporter = dataImporter;
            this.dataExporter = dataExporter;
        }

        public bool DoesGameSaveExist()
        {
            return File.Exists(NoonUtility.GetGameSaveLocation());
        }

        public bool IsSavedGameActive()
        {
            var htSave = RetrieveHashedSave();
            return htSave.ContainsKey(SaveConstants.SAVE_ELEMENTSTACKS) || htSave.ContainsKey(SaveConstants.SAVE_SITUATIONS);
        }

        //for saving from the game over or legacy choice screen, when the player is between active games
        public void SaveInactiveGame()
        {
            var htSaveTable = dataExporter.GetHashtableForExtragameState();
            File.WriteAllText(NoonUtility.GetGameSaveLocation(), htSaveTable.JsonString());

        }

        //for saving from the tabletop
        public void SaveActiveGame(TabletopContainer tabletopContainer)
        {
            var allStacks = tabletopContainer.GetElementStacksManager().GetStacks();
            var allSituationTokens = tabletopContainer.GetAllSituationTokens();
            

            var htSaveTable = dataExporter.GetSaveHashTable(allStacks,
                allSituationTokens);

            File.WriteAllText(NoonUtility.GetGameSaveLocation(), htSaveTable.JsonString());
        }

        public Hashtable RetrieveHashedSave()
        {
            string importJson = File.ReadAllText(NoonUtility.GetGameSaveLocation());
            Hashtable htSave = SimpleJsonImporter.Import(importJson);
            return htSave;
        }


        public void ImportHashedSaveToContainer(TabletopContainer tabletopContainer, Hashtable htSave)
        {
            dataImporter.ImportSavedGameToContainer(tabletopContainer,htSave);
        }
    }
}
