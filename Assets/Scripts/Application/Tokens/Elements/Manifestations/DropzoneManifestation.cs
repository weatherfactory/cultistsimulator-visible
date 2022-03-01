using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Spheres.Angels;
using SecretHistories.Constants.Events;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Ghosts;
using SecretHistories.Manifestations;
using SecretHistories.Services;
using SecretHistories.UI;
using SecretHistories.Spheres;
using SecretHistories.Spheres.Angels;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SecretHistories.Manifestations
{
    [RequireComponent(typeof(RectTransform))]
    public class DropzoneManifestation: BasicManifestation, IManifestation,IPointerClickHandler, IPointerEnterHandler
    {
        


        public bool RequestingNoDrag => false;
        public bool RequestingNoSplit => true;

        [SerializeField]
        private Image Glow;

        private Color32 elementDropzoneColor= new Color32(147, 225, 239, 255);
        private Color32 situationDropzoneColor = new Color32(31, 145, 178, 255);
        private float elementDropzoneHeight = 134f;
        private float situationDropzoneHeight = 160f;
        private float situationDropzoneWidth = 420f;



        private const int MAX_PLACEMENT_ATTEMPTS_FOR_DROPZONE=12;
        private const float SITUATION_SPACING = 20f;
        

        [SerializeField] private List<MinimalDominion> Dominions;


        public void OnPointerEnter(PointerEventData eventData)
        {

            popDropzoneBubbles();

            ExecuteEvents.Execute<IPointerEnterHandler>(transform.parent.gameObject, eventData,
                (parentToken, y) => parentToken.OnPointerEnter(eventData));
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            popDropzoneBubbles();

        }

        private void popDropzoneBubbles()
        {
            foreach (var d in Dominions)
            {
                foreach (var s in d.Spheres)
                {
                    var b = s as BubbleSphere;
                    if (b != null)
                        b.Pop(new Context(Context.ActionSource.Unknown));
                }
            }
        }


        public void DoMove(RectTransform tokenRectTransform)
        {
        }

    
        public void Initialise(IManifestable manifestable)
        {
            foreach (var d in Dominions)
                d.RegisterFor(manifestable);
            var choreographer = gameObject.GetComponentInChildren<WorldAwareRowChoreographer>();
            if(choreographer!= null)
                choreographer.MaxPlacementAttempts = MAX_PLACEMENT_ATTEMPTS_FOR_DROPZONE;

            if (manifestable.EntityId == nameof(Situation))
            {
                RectTransform.sizeDelta = new Vector2(situationDropzoneWidth, situationDropzoneHeight); 
                Glow.color = situationDropzoneColor;
                choreographer.InternalSpacing = SITUATION_SPACING;
            }
            else if (manifestable.EntityId == nameof(ElementStack))
            {
                RectTransform.sizeDelta = new Vector2(RectTransform.sizeDelta.x, elementDropzoneHeight);
                Glow.color = elementDropzoneColor;
            }
            else
                NoonUtility.LogWarning($"Dropzone wants to be for payload type {manifestable.EntityId}, which we don't know about - leaving manifestation appearance at default.");
        }

        public void UpdateVisuals(IManifestable manifestable)
        {
            //
        }


        public void SendNotification(INotification notification)
        {
       //
        }

        public bool HandlePointerClick(PointerEventData eventData, Token token)
        {
            return false;
        }

        public void DisplaySpheres(IEnumerable<Sphere> spheres)
        {
        //
        }

        public IGhost CreateGhost()
        {
            return NullGhost.Create(this);
        }

        public void OverrideIcon(string icon)
        {
            throw new NotImplementedException();
        }


        public void ResetIconAnimation()
        {
            
        }

        public override void Retire(RetirementVFX vfx, Action callbackOnRetired)
        {
            Destroy(gameObject);
            callbackOnRetired();
        }


        public void ShowGlow(bool glowState, bool instant = false)
        {
            NoonUtility.LogWarning(this.GetType().Name + " doesn't support this operation");
        }

        public void UpdateDecayVisuals(float lifetimeRemaining, Element element, float interval, bool currentlyBeingDragged)
        {
            NoonUtility.LogWarning(this.GetType().Name + " doesn't support this operation");
        }

        public void Emphasise()
        {
            NoonUtility.LogWarning(this.GetType().Name + " doesn't support this operation");
        }

        public void Understate()
        {
            NoonUtility.LogWarning(this.GetType().Name + " doesn't support this operation");
        }

        public void BeginIconAnimation()
        {
        }

        public bool CanAnimateIcon()
        {
            return false;
        }

        public void OnBeginDragVisuals(Token token)
        {
        }

        public void OnEndDragVisuals(Token token)
        {
        }


        public void Highlight(HighlightType highlightType, IManifestable manifestable)
        {
        }

        public void Unhighlight(HighlightType highlightType, IManifestable manifestable)
        {
            
        }

        public bool NoPush => false;
        public void Unshroud(bool instant)
        {
        }

        public void Shroud(bool instant)
        {
        }



    }
}
