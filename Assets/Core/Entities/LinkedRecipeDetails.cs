using System.Collections.Generic;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;

namespace Assets.Core.Entities
{
    public class LinkedRecipeDetails : IEntityWithId
    {

        private string _id;

        [FucineId]
        public string Id
        {
            get => _id;
        }

        public void SetId(string id)
        {
            _id = id;
        }


        [FucineValue(100)]
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
            Dictionary<string, string> Challenges)
        {
            Additional = additional;
            _id = id;
            Chance = chance;
            Expulsion = expulsion;
            this.Challenges = Challenges ?? new Dictionary<string, string>();
        }

        public void RefineWithCompendium(ContentImportLogger logger, ICompendium populatedCompendium)
        {
        }
    }
}