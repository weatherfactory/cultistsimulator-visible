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
    public class GameSaveManager
    {
        private IGameDataParser dataParser;
        
        public GameSaveManager(IGameDataParser dataParser)
        {
            this.dataParser = dataParser;
        }

        public void SaveGame(TabletopContainer tabletopContainer,string saveFileName)
        {
            var htSaveTable = dataParser.Export(tabletopContainer.GetElementStacksManager().GetStacks(), tabletopContainer.GetAllSituationTokens());
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
            dataParser.ImportSavedGameToContainer(tabletopContainer,htSave);
        }
    }
}
