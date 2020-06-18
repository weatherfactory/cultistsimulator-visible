using Assets.Core.Fucine;
using Assets.Core.Interfaces;

namespace Assets.Core.Entities
{
    public class Expulsion: IEntityAnonymous
    {
        [FucineAspects]
        public AspectsDictionary Filter { get; set; }
        
        [FucineValue(1)] 
        public int Limit { get; set; }

        public Expulsion()
        {
            Filter = new AspectsDictionary();
        }

        public void RefineWithCompendium(ContentImportLogger logger, ICompendium populatedCompendium)
        {
            
        }
    }
}