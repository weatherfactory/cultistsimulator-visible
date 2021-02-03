using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Commands.SituationCommands;
using Newtonsoft.Json;
using SecretHistories.Commands;
using SecretHistories.Constants;
using SecretHistories.Spheres;


namespace SecretHistories.Infrastructure.Persistence
{
    public abstract class PersistableGameState
    {
        public abstract string GetSaveFileLocation();


        protected List<CharacterCreationCommand> _characterCreationCommands;
        protected List<TokenCreationCommand> _tokenCreationCommands;

        public List<CharacterCreationCommand> GetCharacterCreationCommands()
        {
            return new List<CharacterCreationCommand>(_characterCreationCommands);
        }

        public List<TokenCreationCommand> GetTokenCreationCommands()
        {
            return new List<TokenCreationCommand>(_tokenCreationCommands);
        }

        public virtual void Encaust(IEnumerable<Character> characters, IEnumerable<Sphere> spheres)
        {
            Encaustery<CharacterCreationCommand> characterEncaustery=new Encaustery<CharacterCreationCommand>();
            _characterCreationCommands = new List<CharacterCreationCommand>();

            foreach (var character in characters)
            {
                var encaustedCharacterCommand = characterEncaustery.Encaust(character);
                _characterCreationCommands.Add(encaustedCharacterCommand);
            }
            
            
            Encaustery<TokenCreationCommand> tokenEncaustery = new Encaustery<TokenCreationCommand>();
            _tokenCreationCommands=new List<TokenCreationCommand>();

            foreach (var sphere in spheres)
            {
                var allTokensInSphere = sphere.GetAllTokens();
                foreach (var t in allTokensInSphere)
                {
                    var encaustedTokenCommand = tokenEncaustery.Encaust(t);
                    _tokenCreationCommands.Add(encaustedTokenCommand);
                }
            }
        }



        public virtual bool Exists()
        {
            return File.Exists(GetSaveFileLocation());
        }


        public abstract void DeserialiseFromPersistence();


        public async Task<bool> SaveAsync()
        {
            string json = JsonConvert.SerializeObject(this);

            var saveFilePath = GetSaveFileLocation();

            var writeToFileTask = WriteSaveFile(saveFilePath, json);

            await writeToFileTask;
            return true;
        }

        private async Task WriteSaveFile(string saveFilePath, string JsonToSave)
        {
            var task = Task.Run(() => File.WriteAllText(saveFilePath, JsonToSave));
            await task;
        }

        public virtual void ImportPetromnemeStateAfterTheAncientFashion()
        {
            //do nothing
        }

    }
}
