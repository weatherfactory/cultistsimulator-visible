using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Entities;
using SecretHistories.Abstract;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Services;
using SecretHistories.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SecretHistories.Commands
{
    public class CharacterCreationCommand
    {
        public string Name { get; set; }
        public string Profession { get; set; }
        public Legacy ActiveLegacy { get; set; }
        public Ending EndingTriggered { get; set; }
        public Dictionary<string,int> RecipeExecutions {get; set; }
        public Dictionary<string, string> InProgressHistoryRecords { get; set; }
        public Dictionary<string, string> PreviousCharacterHistoryRecords { get; set; }

        public CharacterCreationCommand()
        {
            RecipeExecutions=new Dictionary<string, int>();
        }

        public Character Execute(Stable stable)
        {
      
            var character = Watchman.Get<PrefabFactory>().CreateLocally<Character>(stable.transform);
            character.name = "Character_" + Name;

            stable.AddNewCharacterAsProtag(character);

            Watchman.Get<Compendium>().SupplyLevers(character);

            return character;
        }


}
}
