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
            Watchman.Get<Heart>().Metapause();
            Watchman.Get<LocalNexus>().DisablePlayerInput(0f);


            var gameGateway = Watchman.Get<GameGateway>();
            await gameGateway.TryDefaultSave();

            Watchman.Get<Heart>().Unmetapause();
            Watchman.Get<LocalNexus>().EnablePlayerInput();
        }


        public void OpenIngress(RectTransform atRectTransform, Ingress ingress)
        {

            Watchman.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 3, GameSpeed = GameSpeed.Paused, WithSFX = false });
            Watchman.Get<LocalNexus>().DoHideHud();
            Watchman.Get<CamOperator>().OnZoomEvent(new ZoomLevelEventArgs{AbsoluteTargetZoomLevel = ZoomLevel.Far});

            PreOtherworldAutosave();

            var otherworldToOpen = Otherworlds.SingleOrDefault(o => o.EntityId == ingress.GetOtherworldId());
            if (otherworldToOpen == null)
                NoonUtility.LogWarning("Can't find otherworld with id " + ingress.GetOtherworldId());
            else
            {
                otherworldToOpen.Show(atRectTransform, ingress);
                _currentOtherworld = otherworldToOpen;
            }

            Watchman.Get<IChronicler>()?.ChronicleOtherworldEntry(ingress.EntityId);
        }


        public void HideCurrentOtherworld()
        {
            if(_currentOtherworld!=null)
            {
                _currentOtherworld.Hide();
                _currentOtherworld = null;
            }

            Watchman.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 3, GameSpeed = GameSpeed.DeferToNextLowestCommand, WithSFX = false });

            Watchman.Get<LocalNexus>().DoShowHud();

        }



    }
}
