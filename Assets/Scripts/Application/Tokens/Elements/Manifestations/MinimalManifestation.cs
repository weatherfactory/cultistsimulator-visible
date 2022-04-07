using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Constants;
using SecretHistories.Constants.Events;
using SecretHistories.Ghosts;
using SecretHistories.Manifestations;
using SecretHistories.Services;
using SecretHistories.Spheres;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SecretHistories.Manifestations
{
    [RequireComponent(typeof(RectTransform))]
    public class MinimalManifestation: BasicManifestation, IManifestation
    {

        public bool RequestingNoDrag => true;
        public bool RequestingNoSplit => true;

        public Type GhostType => typeof(NullGhost);



        public void AnimateTo(float duration, Vector3 startPos, Vector3 endPos, Action<Token> animDone, float startScale, float endScale)
        {
            //do nothing
        }


        public void Initialise(IManifestable manifestable)
        {
        }

        public void UpdateVisuals(IManifestable manifestable, Sphere sphere)
        {
            //
        }

        public void UpdateTimerVisuals(float originalDuration, float durationRemaining, float interval, bool resaturate,
            EndingFlavour signalEndingFlavour)
        {
        }

        public void SendNotification(INotification notification)
        {
        }

        public bool HandlePointerClick(PointerEventData eventData, Token token)
        {
            return false;
        }

        public void DisplaySpheres(IEnumerable<Sphere> spheres)
        {
        }

        public IGhost CreateGhost()
        {
            return NullGhost.Create(this);
        }

        public OccupiesSpaceAs OccupiesSpaceAs() => Enums.OccupiesSpaceAs.Intangible;

        public void OverrideIcon(string icon)
        {
        }
        public void ResetIconAnimation()
        {
            //do nothing
        }

        public override void Retire(RetirementVFX vfx, Action callbackOnRetired)
        {
            Destroy(gameObject); //no vfx for a minimal
            callbackOnRetired();
        }



        public void UpdateDecayVisuals(float lifetimeRemaining, Element element, float interval, bool currentlyBeingDragged)
        {
            }

        public void Unshroud(bool instant)
        {
        }

        public void Shroud(bool instant)
        {
        }

        public void Emphasise()
        {
            //
        }

        public void Understate()
        {
            //
        }

        public void BeginIconAnimation()
        {
            
        }

        public bool CanAnimateIcon()
        {
            return false;
        }



        public void Highlight(HighlightType highlightType, IManifestable manifestable)
        {
        }

        public void Unhighlight(HighlightType highlightType, IManifestable manifestable)
        {
            
        }

        public bool NoPush => true;
        public void DoRevealEffect(bool instant)
        {
        }

        public void DoShroudEffect(bool instant)
        {
        }
    }
}
