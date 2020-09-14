using System;
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
        
        [SerializeField] private PlayerInput playerInput;
        

        public DebugTools _debugTools;


        public void Start()
        {
            //  bool setSomeDefaultBindingsForFirstTime = false;
            foreach (InputAction action in playerInput.currentActionMap.actions)
            {
                if (Registry.Get<ICompendium>().EntityExists<Setting>(action.name))
                {
                    ApplyExistingKeybindOverrides(action);
                }
            }
        }

        private static void ApplyExistingKeybindOverrides(InputAction action)
        {
            var keyBindSetting = Registry.Get<ICompendium>().GetEntityById<Setting>(action.name);
            if (!string.IsNullOrEmpty(keyBindSetting.CurrentValue.ToString()))
            {
                action.ApplyBindingOverride(keyBindSetting.CurrentValue.ToString());
            }
            else
            {
                NoonUtility.Log("Keeping default setting for " + action.name);
            }
        }

        public static bool IsInInputField() {
			return inInputField;
		}

		private static bool inInputField;


		void OnDisable() {
			inInputField = false;
		}


        public void Input_Zoom_Key(InputAction.CallbackContext context)
        {
            float value;
            if (context.started)
                value = context.ReadValue<Single>();
            else
                value = 0;
            
            ZoomEvent.Invoke(new ZoomEventArgs{CurrentZoomInput =value });
        }


        public void Input_Truck_Key(InputAction.CallbackContext context)
        {
            float value;
            if (context.started)
                value = context.ReadValue<Single>();
            else
                value = 0;


            TruckEvent.Invoke(new TruckEventArgs {CurrentTruckInput = value});
        }

        public void Input_Pedestal_Key(InputAction.CallbackContext context)
        {
            float value;
            if (context.started)
                value = context.ReadValue<Single>();
            else
                value = 0;

            PedestalEvent.Invoke(new PedestalEventArgs() { CurrentPedestalInput = value });
        }

        public void Input_Zoom_Scrollwheel(InputAction.CallbackContext context)
        {
            ZoomEvent.Invoke(new ZoomEventArgs { CurrentZoomInput = context.ReadValue<Single>()});
        }




        public void Input_ZoomClose(InputAction.CallbackContext context)
        {
            ZoomEvent.Invoke(new ZoomEventArgs { AbsoluteTargetZoomLevel = ZoomEventArgs.ZOOM_CLOSE });
        }

        public void Input_ZoomMid(InputAction.CallbackContext context)
        {
            ZoomEvent.Invoke(new ZoomEventArgs { AbsoluteTargetZoomLevel = ZoomEventArgs.ZOOM_MID });
        }

        public void Input_ZoomFar(InputAction.CallbackContext context)
        {
            ZoomEvent.Invoke(new ZoomEventArgs { AbsoluteTargetZoomLevel = ZoomEventArgs.ZOOM_FAR });
        }




        public void Input_Pause(InputAction.CallbackContext context)
        {
            if (_debugTools.isActiveAndEnabled)
                return;
            SpeedControlEvent.Invoke(new SpeedControlEventArgs
                    {ControlPriorityLevel = 2, GameSpeed = GameSpeed.Paused, WithSFX = true});
            
        }


        public void Input_NormalSpeed(InputAction.CallbackContext context)
        {
            if (_debugTools.isActiveAndEnabled)
                return;
            SpeedControlEvent.Invoke(new SpeedControlEventArgs
                    {ControlPriorityLevel = 1, GameSpeed = GameSpeed.Normal, WithSFX = true});
                SpeedControlEvent.Invoke(new SpeedControlEventArgs
                    {ControlPriorityLevel = 2, GameSpeed = GameSpeed.Unspecified, WithSFX = false});
            
        }

        public void Input_FastSpeed(InputAction.CallbackContext context)
        {
            if (_debugTools.isActiveAndEnabled)
                return;
            
            SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 1, GameSpeed = GameSpeed.Fast, WithSFX = true });
            SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 2, GameSpeed = GameSpeed.Unspecified, WithSFX = false });
            
        }

        public void Input_ToggleDebug(InputAction.CallbackContext context)
        {
            ToggleDebugEvent.Invoke();
        }

        public void Input_GroupAllStacks(InputAction.CallbackContext context)
        {
            StackCardsEvent.Invoke();
        }

        public void Input_StartRecipe(InputAction.CallbackContext context)
        {
            // Check if the player tried to start a recipe while *not* holding on to a card stack or verb
            // This is to ensure the player doesn't drag an item from a recipe slot before attempting to start a
            // situation, which can lead to strange behaviour
            var elementStack = DraggableToken.itemBeingDragged as ElementStackToken;
            if (elementStack == null || !elementStack.IsInRecipeSlot())
            {
                var situationControllers = Registry.Get<SituationsCatalogue>().GetRegisteredSituations();

                foreach (var controller in situationControllers)
                {
                    if (controller.IsOpen)
                    {
                        controller.AttemptActivateRecipe();
                        break;
                    }
                }
            }
        }
        public void Input_CollectAll(InputAction.CallbackContext context)
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
              ToggleDebugEvent.Invoke();
            }


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
                    ToggleOptionsEvent.Invoke();
                }
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
        }

    }
}
