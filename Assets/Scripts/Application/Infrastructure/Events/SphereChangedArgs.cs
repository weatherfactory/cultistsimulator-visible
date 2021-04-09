using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Spheres;
using SecretHistories.UI;

namespace SecretHistories.Events
{
   public class SphereChangedArgs
    {
        public SphereChangedArgs(Sphere s,Context c)
        {
            Sphere = s;
            Context = c;
        }
        public Sphere Sphere { get; set; }
        
        public Context Context { get; set; }
    }
}
