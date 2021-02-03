using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Fucine;
using SecretHistories.Constants;
using SecretHistories.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SecretHistories.Enums;
using SecretHistories.Infrastructure.Persistence;
using SecretHistories.Spheres;

namespace SecretHistories.UI.Scripts
{


   public class SaveDataImporter : IGameDataImporter
    {

        public void ImportCharacter(PersistableGameState source, Character character)
        {
            throw new NotImplementedException();
        }

        public void ImportTableState(PersistableGameState source, Sphere tabletop)
        {
            throw new NotImplementedException();
        }

        public bool IsSavedGameActive(PersistableGameState source, bool temp)
        {
            throw new NotImplementedException();

        }


        private void ParseSaveFile(string filePath,ContentImportLog log)
        {

                    try
                    {
                        using (StreamReader file = File.OpenText(filePath))
                        using (JsonTextReader reader = new JsonTextReader(file))
                        {
                            while (reader.Read())
                            {
                        
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.LogProblem($"Problem in file {filePath}: {e.Message}");
                    }

        }
        
        
    }
}
