using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Entities;
using Assets.Scripts.Application.Entities.NullEntities;
using Newtonsoft.Json;
using SecretHistories.Abstract;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Services;
using SecretHistories.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SecretHistories.Commands
{
    public class CharacterCreationCommand : IEncaustment
    {
        public string Name { get; set; }
        public string Profession { get; set; }
        public string ActiveLegacyId { get; set; }
        public string EndingTriggeredId { get; set; }
        public Dictionary<string, int> RecipeExecutions { get; set; }
        public DateTime DateTimeCreated { get; set; }
        public Dictionary<string, string> InProgressHistoryRecords { get; set; }=new Dictionary<string, string>();
        public Dictionary<string, string> PreviousCharacterHistoryRecords { get; set; }=new Dictionary<string, string>();

        public CharacterCreationCommand()
        {
            RecipeExecutions = new Dictionary<string, int>();
        }


        /// <summary>
        /// Create an entirely new character with a default name
        /// </summary>
        /// <param name="activeLegacy"></param>
        /// <returns></returns>
        public static CharacterCreationCommand IncarnateFromLegacy(Legacy activeLegacy)
        {
            var cc = new CharacterCreationCommand
            {
                ActiveLegacyId = activeLegacy.Id,
                EndingTriggeredId = Ending.NotEnded().Id,
                Name = Watchman.Get<ILocStringProvider>().Get("UI_CLICK_TO_NAME"),
                Profession = activeLegacy.Label,
                DateTimeCreated=DateTime.Now
            };
            // Registry.Retrieve<Chronicler>().CharacterNameChanged(NoonConstants.DEFAULT_CHARACTER_NAME);//so we never see a 'click to rename' in future history

            var hb = new HistoryBuilder();
            cc.PreviousCharacterHistoryRecords = hb.FillInDefaultPast();
            
            return cc;

        }

        public Character ExecuteToProtagonist(Stable stable)
        {

            var character = stable.InstantiateCharacterInStable();
            
            character.Name = Name; //the data property...
            character.name = "Character_" + Name; //...and the game object name. Let's not do this again, eh
            character.Profession = Profession;

            var compendium = Watchman.Get<Compendium>();

            character.ActiveLegacy = compendium.GetEntityById<Legacy>(ActiveLegacyId);
            character.EndingTriggered = compendium.GetEntityById<Ending>(EndingTriggeredId);
            character.SetCreatedAtTime(DateTimeCreated);
            character.OverwriteExecutionsWith(RecipeExecutions);
            foreach (var inProgressRecord in InProgressHistoryRecords)
                character.SetFutureLegacyEventRecord(inProgressRecord.Key, inProgressRecord.Value);
            foreach(var previousHistoryRecord in PreviousCharacterHistoryRecords)
                character.SetOrOverwritePastLegacyEventRecord(previousHistoryRecord.Key,previousHistoryRecord.Value);

            

            Watchman.Get<Compendium>().SupplyLevers(character);


            return character;
        }



    }
}
