using SecretHistories.Manifestations;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Ghosts
{
    public class NullGhost : AbstractGhost
    {

        public override void ShowAt(Sphere projectInSphere, Vector3 anchoredPosition3D)
        {
            Visible = false; //nope, null ghosts are never visible

        }

        public override void HideIn(Token forToken)
        {
   
            Visible = false;
        }

        public override void Retire()
        {
           //
        }


    }
}