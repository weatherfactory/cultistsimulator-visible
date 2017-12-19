using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.CS.TabletopUI;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public interface IHotkeySubscriber
    {
        void ToggleOptionsVisibility();
        void TogglePause();
        void SetFastForward();
        void SetNormalSpeed();
    }
    public class HotkeyWatcher: MonoBehaviour
    {
        private IHotkeySubscriber _subscriber;
        private DebugTools _debugTools;

        public void Initialise(IHotkeySubscriber subscriber,DebugTools debugTools)
        {
            _subscriber = subscriber;
            _debugTools = debugTools;
        }

        public void WatchForHotkeys()
        {
            if (Input.GetKeyDown("`") || (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Tab)))
                _debugTools.gameObject.SetActive(!_debugTools.isActiveAndEnabled);

            if (!_debugTools.isActiveAndEnabled)
            {
                //...it's nice to be able to type N and M

                if (Input.GetKeyDown(KeyCode.N))
                    _subscriber.SetNormalSpeed();

                if (Input.GetKeyDown(KeyCode.M))
                    _subscriber.SetFastForward();

            }

            if (Input.GetKeyDown(KeyCode.Space))
                _subscriber.TogglePause();

            if (Input.GetKeyDown(KeyCode.Escape))
                _subscriber.ToggleOptionsVisibility();
        }


    }
}
