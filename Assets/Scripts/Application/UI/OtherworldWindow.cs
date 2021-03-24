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
        [SerializeField] private OtherworldAnimation _otherworldAnimation;

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
                Show(args.Payload.GetRectTransform());
        }

        public void Show(Transform effectCenter)
        {
            if (_otherworldAnimation.CanShow() == false)
                return;

            //if (!show) // hide the container
            //    _mapSphere.Show(false);

            _otherworldAnimation.onAnimationComplete += OnShowComplete;
            _otherworldAnimation.SetCenterForEffect(effectCenter);
            _otherworldAnimation.Show(); // starts coroutine that calls onManusMapAnimDone when done
        }

        void OnShowComplete(bool show)
        {
            _otherworldAnimation.onAnimationComplete -= OnShowComplete;

            //if (show) // show the container
            //    _mapSphere.Show(true);

        }

        void OnHideComplete()
        {
            _otherworldAnimation.onAnimationComplete -= OnShowComplete;

            //if (show) // show the container
            //    _mapSphere.Show(true);

        }

    }
}
