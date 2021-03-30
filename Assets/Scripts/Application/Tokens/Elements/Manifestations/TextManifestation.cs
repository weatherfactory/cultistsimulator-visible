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
            NoonUtility.Log("Not implemented for TextManifestation: OnBeginDragVisuals");
        }

        public void OnEndDragVisuals()
        {
            NoonUtility.Log("Not implemented for TextManifestation: OnEndDragVisuals");
        }

        public void Highlight(HighlightType highlightType)
        {
            NoonUtility.Log("Not implemented for TextManifestation: Highlight");
        }

        public void Unhighlight(HighlightType highlightType)
        {
            NoonUtility.Log("Not implemented for TextManifestation: Unhighlight");
        }

        public bool NoPush => true;
        public void Unshroud(bool instant)
        {
            NoonUtility.Log("Not implemented for TextManifestation: Reveal");
        }

        public void Shroud(bool instant)
        {
            NoonUtility.Log("Not implemented for TextManifestation: Shroud");
        }

        public void Emphasise()
        {
            NoonUtility.Log("Not implemented for TextManifestation: Emphasise");
        }

        public void Understate()
        {
            NoonUtility.Log("Not implemented for TextManifestation: Understate");
        }

        public bool RequestingNoDrag => false;
        public void DoMove(RectTransform tokenRectTransform)
        {
            NoonUtility.Log("Not implemented for TextManifestation: ");
        }

        public void SendNotification(INotification notification)
        {
            NoonUtility.Log("Not implemented for TextManifestation: ");
        }

        public bool HandlePointerDown(PointerEventData eventData, Token token)
        {
            NoonUtility.Log("Not implemented for TextManifestation: HandlePointerDown");
            return false;
        }

        public void DisplaySpheres(IEnumerable<Sphere> spheres)
        {
            NoonUtility.Log("Not implemented for TextManifestation: DisplaySpheres");
        }

        public IGhost CreateGhost()
        {
            return new NullGhost();
        }

        public void SetParticleSimulationSpace(Transform transform)
        {
            NoonUtility.Log("Not implemented for TextManifestation: SetParticleSimulationSpace");
        }
    }
}
