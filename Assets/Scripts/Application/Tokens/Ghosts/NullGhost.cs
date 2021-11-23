﻿using SecretHistories.Abstract;
using SecretHistories.Manifestations;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Ghosts
{
    public class NullGhost : MonoBehaviour,IGhost
    {
        public bool Visible { get; private set; }

        public  void ShowAt(Sphere projectInSphere, Vector3 showAtAnchoredPosition3D,RectTransform rectTransform)
        {
            Visible = false; //nope, null ghosts are never visible

        }

        public  void HideIn(Token forToken)
        {
   
            Visible = false;
        }

        public bool TryFulfilPromise(Token token, Context context)
        {
          return false;
        }

        public bool PromiseBlocksCandidateRect(Sphere sphere, Rect candidateRect)
        {
            return false;
        }

        public Rect GetRect()
        {
            return new Rect(0,0,0,0);
        }

        public  void Retire()
        {
           //
        }


        public static IGhost Create(IManifestation parentManifestation)
        {
            var nullGhost = new GameObject().AddComponent<NullGhost>();
            nullGhost.gameObject.transform.SetParent(parentManifestation.Transform);
            return nullGhost;
        }
    }
}