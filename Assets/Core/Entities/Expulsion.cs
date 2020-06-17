using Assets.Core.Fucine;

namespace Assets.Core.Entities
{
    public class Expulsion
    {
        [FucineAspects]
        public AspectsDictionary Filter { get; set; }
        
        [FucineValue(1)] 
        public int Limit { get; set; }

        public Expulsion()
        {
            Limit = 0;
            Filter = new AspectsDictionary();
        }
    }
}