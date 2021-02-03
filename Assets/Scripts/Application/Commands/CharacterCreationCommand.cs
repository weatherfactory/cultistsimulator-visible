using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Entities;
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
        public Legacy ActiveLegacy { get; set; }
        public Ending EndingTriggered { get; set; }
        public Dictionary<string, int> RecipeExecutions { get; set; }
        public Dictionary<string, string> InProgressHistoryRecords { get; set; }=new Dictionary<string, string>();
        public Dictionary<string, string> PreviousCharacterHistoryRecords { get; set; }=new Dictionary<string, string>();

        public CharacterCreationCommand()
        {
            RecipeExecutions = new Dictionary<string, int>();
        }

        public Character Execute(Stable stable)
        {

            var character = Watchman.Get<PrefabFactory>().CreateLocally<Character>(stable.transform);
            character.name = "Character_" + Name;
            character.Profession = Profession;
            character.ActiveLegacy = ActiveLegacy;
            character.EndingTriggered = EndingTriggered;
            character.OverwriteExecutionsWith(RecipeExecutions);
            foreach (var inProgressRecord in InProgressHistoryRecords)
                character.SetFutureLegacyEventRecord(inProgressRecord.Key, inProgressRecord.Value);
            foreach(var previousHistoryRecord in PreviousCharacterHistoryRecords)
                character.SetOrOverwritePastLegacyEventRecord(previousHistoryRecord.Key,previousHistoryRecord.Value);

            stable.AddNewCharacterAsProtag(character);

            Watchman.Get<Compendium>().SupplyLevers(character);

            return character;
        }



    }
}
