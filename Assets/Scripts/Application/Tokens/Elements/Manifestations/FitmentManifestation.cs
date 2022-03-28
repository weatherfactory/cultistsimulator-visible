using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Ghosts;
using SecretHistories.Manifestations;
using SecretHistories.Services;
using SecretHistories.UI;
using UnityEngine.EventSystems;


    namespace SecretHistories.Manifestations
    {
        //Generic furniture of some sort
        public class FitmentManifestation : BasicManifestation, IManifestation
        {
            public bool CanAnimateIcon()
            {
                return false;
            }

            public void BeginIconAnimation()
            {

            }

            public void Initialise(IManifestable manifestable)
            {
                name = GetType().Name + manifestable.Id;

            }

            public void UpdateVisuals(IManifestable manifestable)
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
            }

            public void Understate()
            {
            }

            public bool RequestingNoDrag => false;
            public bool RequestingNoSplit => true;

            public bool HandlePointerClick(PointerEventData eventData, Token token)
            {
                return false;
            }

            public IGhost CreateGhost()
            {
                var newGhost = Watchman.Get<PrefabFactory>()
                    .CreateGhostPrefab(typeof(FitmentGhost), this.RectTransform);
                return newGhost;
            }
        }
    }

