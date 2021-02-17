using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SecretHistories.UI.Scripts;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Services;

using OrbCreationExtensions;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Constants;
using SecretHistories.Enums;
using SecretHistories.Infrastructure.Persistence;
using SecretHistories.Spheres;
using UnityEngine;	// added for debug asserts - CP
using UnityEngine.Analytics;

namespace SecretHistories.Infrastructure
{

    public class TableSaveState : ITableSaveState
    {
        public IEnumerable<ElementStack> TableStacks { get; private set; }
        public List<Situation> Situations { get; private set; }
        public bool IsTableActive()
        {
            return true;
        }

        public MetaInfo MetaInfo { get; }

    }

    public class InactiveTableSaveState : ITableSaveState
    {
        public IEnumerable<ElementStack> TableStacks { get; private set; }
        public List<Situation> Situations { get; private set; }

        public bool IsTableActive()
        {
            return false;
        }

        public MetaInfo MetaInfo { get; }

        public InactiveTableSaveState(MetaInfo metaInfo)
        {
            TableStacks = new List<ElementStack>();
            Situations = new List<Situation>();
            MetaInfo = metaInfo;
        }

    }


    public class GameSaveManager: MonoBehaviour
    {
        private IGameDataImporter dataImporter= new SaveDataImporter();
        private SaveDataExporter dataExporter= new SaveDataExporter();
        private SimpleJSONGameDataImporter simpleJsonGameDataImporter=new SimpleJSONGameDataImporter();

        // Save game safety
        public static int failedSaveCount = 0;
		public static bool saveErrorWarningTriggered = false;	// so that tabletop knows not to unpause

    
        public bool DoesGameSaveExist()
        {
            return File.Exists(NoonUtility.GetGameSaveLocation());
        }

        public bool IsSavedGameActive(PetromnemeGamePersistenceProvider source, bool temp = false)
        {
            return simpleJsonGameDataImporter.IsSavedGameActive(source);
        }

        //copies old version in case of corruption
        private void BackupSave(int index = 0)
        {
			const int MAX_BACKUPS = 5;
			// Back up a number of previous saves
            for (int i=MAX_BACKUPS-1; i>=1; i--)
			{
				if (File.Exists(NoonUtility.GetBackupGameSaveLocation(i)))	//otherwise we can't copy it
	                File.Copy  (NoonUtility.GetBackupGameSaveLocation(i), NoonUtility.GetBackupGameSaveLocation(i+1),true);
			}
			// Back up the main save
			if (File.Exists(NoonUtility.GetGameSaveLocation(index)))	//otherwise we can't copy it
                File.Copy  (NoonUtility.GetGameSaveLocation(index), NoonUtility.GetBackupGameSaveLocation(1),true);
		}


        


    }
}
