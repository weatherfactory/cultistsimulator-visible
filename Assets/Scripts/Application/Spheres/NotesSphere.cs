using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Enums;
using SecretHistories.Spheres;

namespace Assets.Scripts.Application.Spheres
{
    public class NotesSphere: Sphere
    {
        public override SphereCategory SphereCategory => SphereCategory.Notes;
        public virtual bool AllowStackMerge => false;
        
        }
}
