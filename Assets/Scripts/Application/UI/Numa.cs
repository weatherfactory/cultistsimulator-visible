using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Logic;
using Assets.Scripts.Application.Infrastructure.Events;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Spheres;
using SecretHistories.Assets.Scripts.Application.UI;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Infrastructure;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.Tokens.TokenPayloads;
using UnityEngine;

namespace SecretHistories.UI
{
    public class Numa : MonoBehaviour
    {
        [SerializeField] private List<Otherworld> Otherworlds;

        private Otherworld _currentOtherworld;

        public void Awake()
        {
            var w = new Watchman();
            w.Register(this);

            foreach (var o in Otherworlds)
                o.Prepare();
        }

        public bool IsOtherworldActive()
        {
            return _currentOtherworld != null;
        }

        private async void PreOtherworldAutosave()
        {
          //  Watchman.Get<Heart>().Metapause(); //should already be Metapaused - see comment below.
            Watchman.Get<LocalNexus>().DisablePlayerInput(0f);


            var gameGateway = Watchman.Get<GameGateway>();
            await gameGateway.TryDefaultSave();

           // Watchman.Get<Heart>().Unmetapause(); //we don't want to unmetapause when we're heading into an otherworld - see comment re Metapause below
            Watchman.Get<LocalNexus>().EnablePlayerInput();
        }


        public void OpenIngress(RectTransform atRectTransform, Ingress ingress)
        {
            var otherworldToOpen = Otherworlds.SingleOrDefault(o => o.EntityId == ingress.GetOtherworldId());
            if (otherworldToOpen == null)
            {
                NoonUtility.LogWarning($"Can't find otherworld with id '{ingress.GetOtherworldId()}' - retiring ingress");
                ingress.Retire(RetirementVFX.None);
                return;

            }
            Watchman.Get<LocalNexus>().SpeedControlEvent.Invoke(SpeedControlEventArgs.ArgsForPause());
            Watchman.Get<LocalNexus>().DoHideHud();
            Watchman.Get<CamOperator>().OnZoomEvent(new ZoomLevelEventArgs{AbsoluteTargetZoomLevel = ZoomLevel.Far});
            Watchman.Get<Heart>().Metapause();

          //  PreOtherworldAutosave(); Player feedback suggests this is confusing and unhelpful, because of the portal window that opens on load. Disabling for now.

                otherworldToOpen.Show(atRectTransform, ingress);
                _currentOtherworld = otherworldToOpen;
            Watchman.Get<IChronicler>()?.ChronicleOtherworldEntry(ingress.EntityId);
        }


        public void HideCurrentOtherworld()
        {
        
            if (_currentOtherworld!=null)
            {

                _currentOtherworld.Hide();
                _currentOtherworld = null;
            }


            Watchman.Get<LocalNexus>().DoShowHud();
            Watchman.Get<Heart>().Unmetapause();

 


        }



    }
}
