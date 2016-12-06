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

        public void SaveGame(TabletopContainer tabletopContainer,string saveFileName)
        {
            var htSaveTable = dataExporter.ExportStacksAndSituations(tabletopContainer.GetElementStacksManager().GetStacks(), tabletopContainer.GetAllSituationTokens());
            File.WriteAllText(NoonUtility.GetGameSavePath(saveFileName), htSaveTable.JsonString());
        }

        public Hashtable LoadSavedGame(string saveFileName)
        {
            string importJson = File.ReadAllText(NoonUtility.GetGameSavePath(saveFileName));
            Hashtable htSave = SimpleJsonImporter.Import(importJson);
            return htSave;
        }


        public void ImportSavedGameToContainer(TabletopContainer tabletopContainer, Hashtable htSave)
        {
            dataImporter.ImportSavedGameToContainer(tabletopContainer,htSave);
        }
    }
}
