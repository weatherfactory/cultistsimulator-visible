using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Spheres.Thresholds;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Ghosts;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SecretHistories.Manifestations
{
    [RequireComponent(typeof(RectTransform))]
    public class BookcaseManifestation: MonoBehaviour, IManifestation
    {
        public Transform Transform => gameObject.transform;
        public RectTransform RectTransform => gameObject.GetComponent<RectTransform>();

        [SerializeField] List<ShelfDominion> _dominions;

 

        public void Retire(RetirementVFX retirementVfx, Action callback)
        {
            throw new NotImplementedException();
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
            name = "BookcaseManifestation" + manifestable.Id;
            foreach(var d in _dominions)
                d.RegisterFor(manifestable);
        }

        public void UpdateVisuals(IManifestable manifestable)
        {
     
        }

        public void UpdateLocalScale(Vector3 newScale)
        {
            RectTransform.localScale = newScale;
        }

        public void OnBeginDragVisuals()
        {
        }

        public void OnEndDragVisuals()
        {
        }

        public void Highlight(HighlightType highlightType, IManifestable manifestable)
        {
        }

        public void Unhighlight(HighlightType highlightType, IManifestable manifestable)
        {
        }

        public bool NoPush { get; }
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

        public bool RequestingNoDrag { get; }
        public bool RequestingNoSplit => true;

        public void DoMove(RectTransform tokenRectTransform)
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
            var newGhost = Watchman.Get<PrefabFactory>()
                .CreateGhostPrefab(typeof(BookcaseGhost), this.RectTransform);
            return newGhost;
        }

    }
}
