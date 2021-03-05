using SecretHistories.Abstract;
using SecretHistories.Manifestations;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Ghosts
{
    public class NullGhost : IGhost
    {
        public bool Visible { get; private set; }

        public  void ShowAt(Sphere projectInSphere, Vector3 anchoredPosition3D)
        {
            Visible = false; //nope, null ghosts are never visible

        }

        public  void HideIn(Token forToken)
        {
   
            Visible = false;
        }

        public bool TryFulfilPromise(Token token, Context context)
        {
            throw new System.NotImplementedException();
        }

        public  void Retire()
        {
           //
        }


    }
}