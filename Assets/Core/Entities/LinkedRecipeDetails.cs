using System.Collections.Generic;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;

namespace Assets.Core.Entities
{
    public class LinkedRecipeDetails : IEntityKeyed
    {
        
        [FucineId]
        public string Id { get; set; }


        [FucineValue(0)]
        public int Chance { get; set; }

        [FucineValue(false)]
        public bool Additional { get; set; }


        [FucineDict]
        public Dictionary<string, string> Challenges { get; set; }

        [FucineSubEntity(typeof(Expulsion))]
        public Expulsion Expulsion { get; set; }
  

        public LinkedRecipeDetails()
        {
        }

        public LinkedRecipeDetails(string id, int chance, bool additional, Expulsion expulsion,
            Dictionary<string, string> challenges)
        {
            Additional = additional;
            Id = id;
            Chance = chance;
            Expulsion = expulsion;
            Challenges = challenges ?? new Dictionary<string, string>();
        }

        public void RefineWithCompendium(ContentImportLogger logger, ICompendium populatedCompendium)
        {
        }
    }
}