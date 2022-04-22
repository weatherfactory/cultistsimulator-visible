using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Tokens.Ghosts;
using SecretHistories.Enums;
using SecretHistories.Ghosts;
using SecretHistories;
using SecretHistories.Assets.Scripts.Application.Spheres.Dominions;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace SecretHistories.Manifestations
{
 [RequireComponent(typeof(RectTransform))]
    public class MortalManifestation: BasicManifestation, IManifestation
    {
        [SerializeField]
        private GameObject _emphasisGlow;

        [SerializeField] private Image _artwork;
        [SerializeField] protected List<AnnexDominion> _dominions;

        public override void Retire(RetirementVFX retirementVfx, Action callback)
        {
            throw new NotImplementedException();
        }

        public bool CanAnimateIcon()
        {
            throw new NotImplementedException();
        }

        public void BeginIconAnimation()
        {
          //
        }

        public void Initialise(IManifestable manifestable)
        {
           UpdateVisuals(manifestable,NullSphere.Create());
           name = GetType().Name + manifestable.Id;
           foreach (var d in _dominions)
               d.RegisterFor(manifestable);
        }


        public void UpdateVisuals(IManifestable manifestable, Sphere sphere)
        {
            var sprite = ResourcesManager.GetSpriteForSomeone(manifestable.Icon);
            _artwork.sprite = sprite;
        }

  


        public void Highlight(HighlightType highlightType, IManifestable manifestable)
        {
        //
        }

        public void Unhighlight(HighlightType highlightType, IManifestable manifestable)
        {
            //
        }

        public bool NoPush { get; }
        public void Unshroud(bool instant)
        {
           //
        }

        public void Shroud(bool instant)
        {
            //
        }

        public void Emphasise()
        {
          _emphasisGlow.SetActive(true);
        }

        public void Understate()
        {
          _emphasisGlow.SetActive(false);

        }

        public bool RequestingNoDrag => true;
        public bool RequestingNoSplit { get; }

        //eg Someones which can only be directed, not dragged


        public bool HandlePointerClick(PointerEventData eventData, Token token)
        {
            return false;
        }

        public IGhost CreateGhost()
        {
            var newGhost = Watchman.Get<PrefabFactory>()
                .CreateGhostPrefab(typeof(MortalGhost), this.RectTransform);
            return newGhost;
        }

        public OccupiesSpaceAs OccupiesSpaceAs() => Enums.OccupiesSpaceAs.Someone;
    }
}
