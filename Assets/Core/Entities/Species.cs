using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Fucine;
using Assets.Core.Fucine.DataImport;

namespace Assets.Core.Entities
{
    [FucineImportable("species")]
    public class Species: AbstractEntity<Species>
    {

        [FucineList]
        public List<SphereSpecification> Spheres { get; set; }



        public Species(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {

        }


        protected override void OnPostImportForSpecificEntity(ContentImportLog log, ICompendium populatedCompendium)
        {
            //
        }


    }
}
