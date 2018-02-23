using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public class MapController: MonoBehaviour
    {
        private MapTokenContainer _mapTokenContainer;
        //private TabletopBackground _mapBackground;
        private MapAnimation _mapAnimation;

        public void Initialise(MapTokenContainer mapTokenContainer, TabletopBackground mapBackground, MapAnimation mapAnimation) {
            mapBackground.gameObject.SetActive(false);

            mapTokenContainer.gameObject.SetActive(false);
            _mapTokenContainer = mapTokenContainer;

            //_mapBackground = mapBackground;

            _mapAnimation = mapAnimation;
            mapAnimation.Init();
        }

        public void ShowMansusMap(Transform effectCenter, bool show = true)
        {
            if (_mapAnimation.CanShow(show) == false)
                return;

            if (!show) // hide the container
                _mapTokenContainer.Show(false);

            // TODO: should probably lock interface? No zoom, no tabletop interaction. Check EndGameAnim for ideas

            _mapAnimation.onAnimDone += OnMansusMapAnimDone;
            _mapAnimation.SetCenterForEffect(effectCenter);
            _mapAnimation.Show(show); // starts coroutine that calls onManusMapAnimDone when done
            _mapAnimation.Show(show);
        }

        void OnMansusMapAnimDone(bool show)
        {
            _mapAnimation.onAnimDone -= OnMansusMapAnimDone;

            if (show) // show the container
                _mapTokenContainer.Show(true);
            // TODO: should probably unlock interface? No zoom, no tabletop interaction
        }

        public void HideMansusMap(Transform effectCenter, IElementStack stack)
        {
            Debug.Log("Dropped Stack " + (stack != null ? stack.Id : "NULL"));
            ShowMansusMap(effectCenter, false);
        }

#if DEBUG
        public void CloseMap() {
            Registry.Retrieve<TabletopManager>().HideMansusMap(_mapTokenContainer.GetDoor().transform);
        }
#endif
    }
}
