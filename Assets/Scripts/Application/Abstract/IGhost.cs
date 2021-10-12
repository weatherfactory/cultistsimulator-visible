using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Ghosts;
using SecretHistories.Manifestations;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Abstract
{
   public interface IGhost
    {
        
        bool Visible { get; }
        void ShowAt(Sphere projectInSphere, Vector3 anchoredPosition3D);
        void HideIn(Token forToken);
        bool TryFulfilPromise(Token token,Context context);
        bool PromiseBlocksCandidateRect(Sphere sphere, Rect candidateRect);
        Rect GetRect();
        void Retire();
    }
}
