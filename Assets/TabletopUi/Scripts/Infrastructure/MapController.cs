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
        //private MapContainsTokens _mapContainsTokens;
        //private TabletopBackground _mapBackground;
        private MapAnimation _mapAnimation;

        public void Initialise(MapContainsTokens mapContainsTokens, TabletopBackground mapBackground, MapAnimation mapAnimation) {
            //_mapContainsTokens = mapContainsTokens;
            //_mapBackground = mapBackground;
            _mapAnimation = mapAnimation;
        }

        public void ShowMansusMap(Transform effectCenter, bool show = true)
        {
            if (_mapAnimation.CanShow(show) == false)
                return;

            // TODO: should probably lock interface? No zoom, no tabletop interaction. Check EndGameAnim for ideas

            _mapAnimation.onAnimDone += OnMansusMapAnimDone;
            _mapAnimation.SetCenterForEffect(effectCenter);
            _mapAnimation.Show(show); // starts coroutine that calls onManusMapAnimDone when done
            _mapAnimation.Show(show);
        }

        void OnMansusMapAnimDone(bool show)
        {
            _mapAnimation.onAnimDone -= OnMansusMapAnimDone;
            // TODO: should probably unlock interface? No zoom, no tabletop interaction
        }

        public void HideMansusMap(Transform effectCenter, IElementStack stack)
        {
            Debug.Log("Dropped Stack " + (stack != null ? stack.Id : "NULL"));
            ShowMansusMap(effectCenter, false);
        }
    }
}
