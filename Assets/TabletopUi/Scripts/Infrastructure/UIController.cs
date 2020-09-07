using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using Noon;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public class UIController: LocalNexus
    {

        public UnityEvent foo;


        public DebugTools _debugTools;
        

        public static bool IsInInputField() {
			return inInputField;
		}

		private static bool inInputField;


		void OnDisable() {
			inInputField = false;
		}

        public void Input_Pause(InputAction.CallbackContext context)
        {
            if (!_debugTools.isActiveAndEnabled)
            {
                Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs
                    {ControlPriorityLevel = 2, GameSpeed = GameSpeed.Paused, WithSFX = true});
            }
        }


        public void Input_NormalSpeed(InputAction.CallbackContext context)
        {
            if (!_debugTools.isActiveAndEnabled)
            {
                Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs
                    {ControlPriorityLevel = 1, GameSpeed = GameSpeed.Normal, WithSFX = true});
                Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs
                    {ControlPriorityLevel = 2, GameSpeed = GameSpeed.Unspecified, WithSFX = false});
            }
        }

        public void Input_FastSpeed(InputAction.CallbackContext context)
        {
            if (!_debugTools.isActiveAndEnabled)
            {
                Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 1, GameSpeed = GameSpeed.Fast, WithSFX = true });
                Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 2, GameSpeed = GameSpeed.Unspecified, WithSFX = false });
            }
        }

        public void Update()
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
              Registry.Get<LocalNexus>().ToggleDebugEvent.Invoke();
            }


            //...it's nice to be able to type N and M

            //if (!_debugTools.isActiveAndEnabled)
            //{

            //    if (Keyboard.current.nKey.wasPressedThisFrame)
            //    //if (Input.GetKeyDown(KeyCode.N))
            //    {
            //       Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel =1 , GameSpeed = GameSpeed.Normal, WithSFX =true });
            //       Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 2, GameSpeed = GameSpeed.Unspecified, WithSFX = false });
            //    }
            //    //if (Input.GetKeyDown(KeyCode.M))
            //    if (Keyboard.current.mKey.wasPressedThisFrame)
            //    {
            //        Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 1, GameSpeed = GameSpeed.Fast, WithSFX = true});
            //        Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 2, GameSpeed = GameSpeed.Unspecified, WithSFX = false });
            //    }
            //}



            //if (Keyboard.current.spaceKey.wasPressedThisFrame)
            //    Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 2, GameSpeed = GameSpeed.Paused, WithSFX = true });

            if (IsPressingAbortHotkey())	// Uses Keycode.Escape by default, for the benefit of anyone trying to search for this in future :)
			{
				// Check for open situation windows and close them first
				bool windowWasOpen = false;
				var situationControllers = Registry.Get<SituationsCatalogue>().GetRegisteredSituations();

				foreach (var controller in situationControllers) {
					if (controller.IsOpen) {
						controller.CloseWindow();
						windowWasOpen = true;
						break;
					}
				}

				if (!windowWasOpen)	// Only summon options if no windows to clear
				{
                    Registry.Get<LocalNexus>().ToggleOptionsEvent.Invoke();
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
					var situationControllers = Registry.Get<SituationsCatalogue>().GetRegisteredSituations();

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
                var situationControllers = Registry.Get<SituationsCatalogue>().GetRegisteredSituations();

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
	            Registry.Get<TabletopManager>().GroupAllStacks();
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
            return Keyboard.current.escapeKey.wasPressedThisFrame;
            //   return Input.GetButtonDown("Cancel");
        }

    }
}
