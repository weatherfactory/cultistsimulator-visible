using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Infrastructure
{

    public class HotkeyWatcher: MonoBehaviour
    {
        private SpeedController _speedController;
        private DebugTools _debugTools;
        private OptionsPanel _optionsPanel;

        public void Initialise(SpeedController speedController,DebugTools debugTools,OptionsPanel optionsPanel)
        {
            _speedController=speedController;
            _debugTools = debugTools;
            _optionsPanel = optionsPanel;
        }

        public void WatchForGameplayHotkeys()
        {
            if (!enabled)
                return;

            if (Input.GetKeyDown("`") || (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Tab)))
            { 
                _debugTools.gameObject.SetActive(!_debugTools.isActiveAndEnabled);
                var situationsCatalogue = Registry.Retrieve<SituationsCatalogue>();
                foreach (var sc in situationsCatalogue.GetRegisteredSituations())
                {
                    if(_debugTools.isActiveAndEnabled && sc.IsOpen)
                        sc.SetEditorActive(true);
                    else
                    sc.SetEditorActive(false);
                }
            }
            if (!_debugTools.isActiveAndEnabled)
            {
                //...it's nice to be able to type N and M

                if (Input.GetKeyDown(KeyCode.N))
                    _speedController.SetNormalSpeed();

                if (Input.GetKeyDown(KeyCode.M))
                    _speedController.SetFastForward();
            }


            if (Input.GetKeyDown(KeyCode.Space))
				_speedController.TogglePause();

            if (Input.GetKeyDown(KeyCode.Escape))
                _optionsPanel.ToggleVisibility();
        }

        public bool IsPressingAbortHotkey() {
            if (Input.GetKeyDown(KeyCode.Escape))
                return true;

            return false;
        }

    }
}
