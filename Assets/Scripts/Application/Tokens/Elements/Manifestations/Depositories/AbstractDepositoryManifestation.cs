using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Ghosts;
using SecretHistories;
using SecretHistories.Manifestations;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SecretHistories.Manifestations
{
    public abstract class AbstractDepositoryManifestation: BasicManifestation
    {

        [SerializeField] protected List<ShelfDominion> _dominions;

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
            name = GetType().Name + manifestable.Id;
            foreach (var d in _dominions)
                d.RegisterFor(manifestable);
        }

        public void UpdateVisuals(IManifestable manifestable)
        {

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
                .CreateGhostPrefab(typeof(DepositoryGhost), this.RectTransform);
            return newGhost;
        }
    }
}
