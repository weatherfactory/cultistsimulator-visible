using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Fucine;
using Assets.Core.Fucine.DataImport;
using Assets.CS.TabletopUI;
using Assets.Scripts.Spheres.Angels;
using Assets.TabletopUi.Scripts.Infrastructure;

namespace Assets.Scripts.Entities
{
   public class AngelSpecification: AbstractEntity<AngelSpecification>
    {
        [FucineValue]
        public string Choir { get; set; }

        [FucineValue]
        public string WatchOver { get; set; }

        protected override void OnPostImportForSpecificEntity(ContentImportLog log, Compendium populatedCompendium)
        {
        }

        public AngelSpecification()
        {}

        public AngelSpecification(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {

        }

        public IAngel MakeAngel()
        {

            if (Choir == "tidy")
            {
                var angel = new TidyAngel(); ;
                var watchOverSpherePath = new SpherePath(WatchOver);
                var watchOverSphere = Registry.Get<SphereCatalogue>().GetSphereByPath(watchOverSpherePath);
                angel.SetWatch(watchOverSphere);
                return angel;
            }

            else
            {
                throw new ApplicationException("Unknown angel choir: " + Choir);
            }
        }


    }
}
