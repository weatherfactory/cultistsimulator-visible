using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Infrastructure.Events;
using SecretHistories.Abstract;
using UnityEngine;

namespace SecretHistories.UI
{
    public class OtherworldWindow: MonoBehaviour
    {
        [SerializeField] private MapAnimation _mapAnimation;

        private ITokenPayload _portal;

        private bool _isOpen;

        public void Attach(ITokenPayload toTokenPayload)
        {
            _portal = toTokenPayload;
            _portal.OnChanged += OnPayloadChanged;
        }

        private void OnPayloadChanged(TokenPayloadChangedArgs args)
        {
            if(!_isOpen && args.Payload.IsOpen)
                ShowMansusMap(args.Payload.GetRectTransform());
        }

        public void ShowMansusMap(Transform effectCenter, bool show = true)
        {
            if (_mapAnimation.CanShow(show) == false)
                return;

            //if (!show) // hide the container
            //    _mapSphere.Show(false);

            _mapAnimation.onAnimDone += OnMansusMapAnimDone;
            _mapAnimation.SetCenterForEffect(effectCenter);
            _mapAnimation.Show(show); // starts coroutine that calls onManusMapAnimDone when done
            _mapAnimation.Show(show);
        }

        void OnMansusMapAnimDone(bool show)
        {
            _mapAnimation.onAnimDone -= OnMansusMapAnimDone;

            //if (show) // show the container
            //    _mapSphere.Show(true);

        }

    }
}
