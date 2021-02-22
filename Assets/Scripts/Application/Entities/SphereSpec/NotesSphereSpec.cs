using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Spheres;
using SecretHistories.Spheres;

namespace SecretHistories.Entities
{
    public class NotesSphereSpec: SphereSpec
    {
        public override Type SphereType => typeof(NotesSphere);

        public NotesSphereSpec(int index): base(new NotesSphereSpecIdentifierStrategy(index))
        {
            
        }
    }
}
