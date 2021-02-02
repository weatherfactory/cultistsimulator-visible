using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Services;
using SecretHistories.UI;

namespace SecretHistories.Commands
{
    public class CharacterCreationCommand
    {
        public string Name { get; set; }
        public string Profession { get; set; }
        public Legacy ActiveLegacy { get; set; }
        public Ending EndingTriggered { get; set; }
        public Dictionary<string,int> RecipeExecutions {get; set; }
        public Dictionary<string, string> InProgressHistoryRecords;
        public Dictionary<string, string> PreviousCharacterHistoryRecords;

        public CharacterCreationCommand()
        {
            RecipeExecutions=new Dictionary<string, int>();
        }

        public Character Execute(ICharacterHost characterHost)
        {
            var character = Watchman.Get<PrefabFactory>().CreateLocally<Character>(characterHost.transform);
            
            Watchman.Get<Compendium>().SupplyLevers(character);

            return character;
        }


}
}
