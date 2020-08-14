using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Noon;
using TabletopUi.Scripts.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.TabletopUi.Scripts.Infrastructure
{

    public class HotkeyWatcher: MonoBehaviour
    {
        private SpeedController _speedController;
        private DebugTools _debugTools;
        private OptionsPanel _optionsPanel;

        public static bool IsInInputField() {
			return inInputField;
		}

		private static bool inInputField;

        public void Initialise(SpeedController speedController,DebugTools debugTools,OptionsPanel optionsPanel)
        {
            _speedController=speedController;
            _debugTools = debugTools;
            _optionsPanel = optionsPanel;
        }

		void OnDisable() {
			inInputField = false;
		}

        public void WatchForGameplayHotkeys()
        {
            if (!enabled)
                return;

            // Process any debug tools-specific keys first
            if (_debugTools!=null && _debugTools.isActiveAndEnabled && _debugTools.ProcessInput())
                return;

			UpdateInputFieldState();

			if (IsInInputField())
				return;

	        if (((Input.GetKeyDown("`") || Input.GetKeyDown(KeyCode.Quote)) && Input.GetKey(KeyCode.LeftControl) ))
            {
                _debugTools.gameObject.SetActive(!_debugTools.isActiveAndEnabled);
                {
                    if(Config.OldConfig.Instance.knock)
                        _debugTools.btnTriggerAchievement.gameObject.SetActive(true);

                }


                var situationsCatalogue = Registry.Retrieve<SituationsCatalogue>();
                foreach (var sc in situationsCatalogue.GetRegisteredSituations())
                {
                    if(_debugTools.isActiveAndEnabled && sc.IsOpen)
                        sc.SetEditorActive(true);
                    else
                    sc.SetEditorActive(false);
                }
            }

            try
            {
                if (!_debugTools.isActiveAndEnabled)
                {
                    //...it's nice to be able to type N and M

                    if (Input.GetKeyDown(KeyCode.N))
                        _speedController.SetNormalSpeed();

                    if (Input.GetKeyDown(KeyCode.M))
                        _speedController.SetFastForward();
                }
            }

            catch (Exception e)
            {
                NoonUtility.Log("Problem with debug tools: " + e.Message);
            }

            if (Input.GetButtonDown("Pause"))
				_speedController.TogglePause();

            if (IsPressingAbortHotkey())	// Uses Keycode.Escape by default, for the benefit of anyone trying to search for this in future :)
			{
				// Check for open situation windows and close them first
				bool windowWasOpen = false;
				var situationControllers = Registry.Retrieve<SituationsCatalogue>().GetRegisteredSituations();

				foreach (var controller in situationControllers) {
					if (controller.IsOpen) {
						controller.CloseWindow();
						windowWasOpen = true;
						break;
					}
				}

				if (!windowWasOpen)	// Only summon options if no windows to clear
				{
					_optionsPanel.ToggleVisibility();
				}
			}

            // Check if the player tried to start a recipe while *not* holding on to a card stack or verb
            // This is to ensure the player doesn't drag an item from a recipe slot before attempting to start a
            // situation, which can lead to strange behaviour
			if ((int)Input.GetAxis("Start Recipe")>0)
			{
				var elementStack = DraggableToken.itemBeingDragged as ElementStackToken;
				if (elementStack == null || !elementStack.IsInRecipeSlot())
				{
					var situationControllers = Registry.Retrieve<SituationsCatalogue>().GetRegisteredSituations();

					foreach (var controller in situationControllers) {
						if (controller.IsOpen) {
							controller.AttemptActivateRecipe();
							break;
						}
					}
				}
			}

            if ((int)Input.GetAxis("Collect All")>0)
            {
                var situationControllers = Registry.Retrieve<SituationsCatalogue>().GetRegisteredSituations();

                foreach (var controller in situationControllers)
                {
                    if (controller.IsOpen)
                    {
                        controller.DumpAllResults();
                        break;
                    }
                }
            }

            if (Input.GetButtonDown("Stack Cards"))
            {
	            Registry.Retrieve<ITabletopManager>().GroupAllStacks();
            }
        }

		void UpdateInputFieldState() {
			if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
			{
			    if (EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null)
			        inInputField = true;

			    if (EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() != null)
			        inInputField = true;

			}
			else {
				inInputField = false;
			}
		}

        public bool IsPressingAbortHotkey()
        {
            return Input.GetButtonDown("Cancel");
        }

    }
}
