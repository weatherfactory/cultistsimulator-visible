using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Constants;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Ghosts;
using SecretHistories.Spheres;
using SecretHistories.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SecretHistories.Manifestations

{
    [RequireComponent(typeof(RectTransform))]
    public class TextManifestation : MonoBehaviour, IManifestation
    {
        public Transform Transform => gameObject.transform;
        public RectTransform RectTransform => gameObject.GetComponent<RectTransform>();
        
        [SerializeField] private TMP_Text textComponent;
        public void Retire(RetirementVFX retirementVfx, Action callback)
        {
            NoonUtility.Log("Not implemented for TextManifestation: ");
        }

        public bool CanAnimateIcon()
        {
            return false;
        }

        public void BeginIconAnimation()
        {
            //
        }

        public void Initialise(IManifestable manifestable)
        {
            var description=manifestable.GetIllumination(NoonConstants.TLG_NOTES_DESCRIPTION_KEY);
            textComponent.text= description;
        }

  
        public void UpdateVisuals(IManifestable manifestable)
        {
            var description = manifestable.GetIllumination(NoonConstants.TLG_NOTES_DESCRIPTION_KEY);
            textComponent.text = description;
        }

        public void OnBeginDragVisuals()
        {
        }

        public void OnEndDragVisuals()
        {
        }

        public void Highlight(HighlightType highlightType)
        {
        }

        public void Unhighlight(HighlightType highlightType)
        {
        }

        public bool NoPush => true;
        public void Unshroud(bool instant)
        {
        
        }

        public void Shroud(bool instant)
        {
            
        }

        public void Emphasise()
        {
           
        }

        public void Understate()
        {
        }

        public bool RequestingNoDrag => false;
        public bool RequestingNoSplit => false;

        public void DoMove(RectTransform tokenRectTransform)
        {
        }

        public void SendNotification(INotification notification)
        {
        }

        public bool HandlePointerDown(PointerEventData eventData, Token token)
        {
            return false;
        }

        public void DisplaySpheres(IEnumerable<Sphere> spheres)
        {
        }

        public IGhost CreateGhost()
        {
            return new NullGhost();
        }

    }
}
