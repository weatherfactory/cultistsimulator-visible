using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Assets.Scripts.Application.Tokens.TravelItineraries;
using SecretHistories.Ghosts;
using SecretHistories;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Abstract
{
   public interface IGhost
    {
        
        bool Visible { get; }
        void ShowAt(Sphere projectInSphere, Vector3 showAtAnchoredPosition3D,RectTransform tokenRectTransform);
        void HideIn(Token forToken);
        bool TryFulfilPromise(Token token,Context context);
        TokenItinerary GetItineraryForFulfilment(Token token);
        bool PromiseBlocksCandidateRect(Sphere sphere, Rect candidateRect);
        Rect GetRect();
        void Retire();
        void Understate();
        void Emphasise();

        void UpdateVisuals(IManifestable manifestable);
        void UpdateVisuals(IManifestable manifestable,Sphere sphere);
    }
}
