using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Fucine;
using SecretHistories.Infrastructure;
using SecretHistories.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SecretHistories.UI.Scripts
{


   public class SaveDataImporter : IGameDataImporter
    {

        public void ImportCharacter(SourceForGameState source, Character character)
        {
            throw new NotImplementedException();
        }

        public void ImportTableState(SourceForGameState source, Sphere tabletop)
        {
            throw new NotImplementedException();
        }

        public bool IsSavedGameActive(SourceForGameState source, bool temp)
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
