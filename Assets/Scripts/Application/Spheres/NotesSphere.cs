using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Spheres;
using SecretHistories.UI;

namespace Assets.Scripts.Application.Spheres
{
    public class NotesSphere: Sphere
    {
        public override SphereCategory SphereCategory => SphereCategory.Notes;
        public override bool AllowStackMerge => false;

        public override void SetUpWithSphereSpecAndPath(SphereSpec sphereSpec, SpherePath pathForThisThreshold)
        {
            SphereIdentifier = sphereSpec.Id;
            gameObject.name = SphereIdentifier; //this could be more usefully frequent in other sphere implementations
        }



        public override List<SphereSpec> GetChildSpheresSpecsToAddIfThisTokenAdded(Token t, SpheresWrangler s)
        {
            var sphereSpec = new SphereSpec(new NotesSphereSpecIdentifierStrategy(s.GetSpheresCurrentlyWrangledCount()));
            var sphereSpecList=new List<SphereSpec>();

            sphereSpecList.Add(sphereSpec);
            return sphereSpecList;
        }
    }
}
