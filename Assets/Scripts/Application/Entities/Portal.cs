using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Fucine;
using SecretHistories.Fucine.DataImport;

namespace SecretHistories.Entities
{

    [FucineImportable("portals")]
    public class Portal: AbstractEntity<Portal>
    {
        [FucineValue(DefaultValue = ".", Localise = true)]
        public string Label { get; set; }

        [FucineValue(DefaultValue = ".", Localise = true)]
        public string Description { get; set; }

        [FucineValue(DefaultValue = ".", Localise = true)]
        public virtual string Icon { get; set; }

        [FucineValue]
        public virtual string OtherworldId { get; set; }

        [FucineValue]
        public virtual string EgressId { get; set; }


        [FucineList(Localise = true)]
        public List<LinkedRecipeDetails> Consequences { get; set; }



        public Portal()
        {
        }

        public Portal(string id): base()
        {
            _id = id;
        }

        public Portal(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {
        }

        protected override void OnPostImportForSpecificEntity(ContentImportLog log, Compendium populatedCompendium)
        {
            foreach (var c in Consequences)
                c.OnPostImport(log, populatedCompendium);
        }

        public string DefaultUniqueTokenId()
        {
            int identity = FucineRoot.Get().IncrementedIdentity();
            string uniqueId = $"!{Id}_{identity}";
            return uniqueId;
        }

        public static Portal CreateEndingPortal(Ending ending,string endingOtherworldId)
        {
            var endingPortal = new Portal(ending.Id) {OtherworldId = endingOtherworldId,Icon="winter"};
            return endingPortal;
        }
    }
}
