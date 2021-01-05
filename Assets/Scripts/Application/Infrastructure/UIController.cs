﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.UI;
using SecretHistories.Interfaces;
using SecretHistories.Services;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace SecretHistories.Infrastructure
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
                if (Registry.Get<Compendium>().EntityExists<Setting>(action.name))
                {
                    ApplyExistingKeybindOverrides(action);
                }
            }
        }

        private static void ApplyExistingKeybindOverrides(InputAction action)
        {
            var keyBindSetting = Registry.Get<Compendium>().GetEntityById<Setting>(action.name);
            if (!string.IsNullOrEmpty(keyBindSetting.CurrentValue.ToString()))
            {
                action.ApplyBindingOverride(keyBindSetting.CurrentValue.ToString());
            }
            else
            {
                NoonUtility.Log("Keeping default setting for " + action.name);
            }
        }


        public void Input_Zoom_Key(InputAction.CallbackContext context)
        {
            if (IsEditingText())
                return;

            float value;
            if (context.started)
                value = context.ReadValue<Single>();
            else
                value = 0;
            
            ZoomEvent.Invoke(new ZoomEventArgs{CurrentZoomInput =value });
        }


        public void Input_Truck_Key(InputAction.CallbackContext context)
        {
            if (IsEditingText())
                return;

            float value;
            if (context.started)
                value = context.ReadValue<Single>();
            else
                value = 0;


            TruckEvent.Invoke(new TruckEventArgs {CurrentTruckInput = value});
        }

        public void Input_Pedestal_Key(InputAction.CallbackContext context)
        {
            if (IsEditingText())
                return;

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
            if (IsEditingText())
                return;

            ZoomEvent.Invoke(new ZoomEventArgs { AbsoluteTargetZoomLevel = ZoomEventArgs.ZOOM_CLOSE });
        }

        public void Input_ZoomMid(InputAction.CallbackContext context)
        {
            if (IsEditingText())
                return;

            ZoomEvent.Invoke(new ZoomEventArgs { AbsoluteTargetZoomLevel = ZoomEventArgs.ZOOM_MID });
        }

        public void Input_ZoomFar(InputAction.CallbackContext context)
        {
            if (IsEditingText())
                return;

            ZoomEvent.Invoke(new ZoomEventArgs { AbsoluteTargetZoomLevel = ZoomEventArgs.ZOOM_FAR });
        }




        public void Input_Pause(InputAction.CallbackContext context)
        {
            
            if (IsEditingText() || _debugTools.isActiveAndEnabled || !context.started)
                return;

            if (_debugTools.isActiveAndEnabled)
                return;
            SpeedControlEvent.Invoke(new SpeedControlEventArgs
                {ControlPriorityLevel = 2, GameSpeed = GameSpeed.Paused, WithSFX = true});

        }


        public void Input_NormalSpeed(InputAction.CallbackContext context)
        {
            if (IsEditingText() ||_debugTools.isActiveAndEnabled || !context.started)
                return;
            SpeedControlEvent.Invoke(new SpeedControlEventArgs
                    {ControlPriorityLevel = 1, GameSpeed = GameSpeed.Normal, WithSFX = false});
                SpeedControlEvent.Invoke(new SpeedControlEventArgs
                    {ControlPriorityLevel = 2, GameSpeed = GameSpeed.DeferToNextLowestCommand, WithSFX = true});

                //SoundManager.PlaySfx("UIPauseEnd");

        }

        public void Input_FastSpeed(InputAction.CallbackContext context)
        {
            if (IsEditingText() || _debugTools.isActiveAndEnabled || !context.started)
                return;

            SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 1, GameSpeed = GameSpeed.Fast, WithSFX = false });
            SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 2, GameSpeed = GameSpeed.DeferToNextLowestCommand, WithSFX = true });
            
        }

        public void Input_ToggleDebug(InputAction.CallbackContext context)
        {
            ToggleDebugEvent.Invoke();
        }

        public void Input_GroupAllStacks(InputAction.CallbackContext context)
        {
            if (IsEditingText())
                return;

            StackCardsEvent.Invoke();
        }

        public void Input_StartRecipe(InputAction.CallbackContext context)
        {
            if (IsEditingText())
                return;

            var situations = Registry.Get<SituationsCatalogue>().GetRegisteredSituations();

            foreach (var s in situations)
            {
                if (s.IsOpen)
                {
                    s.TryStart();
                    break;
                }
            }
        }
        public void Input_CollectAll(InputAction.CallbackContext context)
        {
            if (IsEditingText())
                return;

            var situations = Registry.Get<SituationsCatalogue>().GetRegisteredSituations();

            foreach (var s in situations)
            {
                if (s.IsOpen)
                {
                    s.Conclude();
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
          
                if (IsEditingText())
                    return;
            if(Keyboard.current.backquoteKey.wasPressedThisFrame || Keyboard.current.quoteKey.wasPressedThisFrame)
	        //if (((Input.GetKeyDown("`") || Input.GetKeyDown(KeyCode.Quote)) && Input.GetKey(KeyCode.LeftControl) ))
            {
              ToggleDebugEvent.Invoke();
              Registry.Get<Concursum>().ToggleSecretHistory();
            }


            if (IsPressingAbortHotkey())	// Uses Keycode.Escape by default, for the benefit of anyone trying to search for this in future :)
			{
				// Check for open situation windows and close them first
				bool windowWasOpen = false;
				var situations = Registry.Get<SituationsCatalogue>().GetRegisteredSituations();

				foreach (var situation in situations) {
					if (situation.IsOpen) {
						situation.Close();
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

		public bool IsEditingText() {
			if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
			{
			    if (EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null)
			        return true;

			    if (EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() != null)
                    return true;

                return false;
            }
			
            return false;
        }

        public bool IsPressingAbortHotkey()
        {
            return Keyboard.current.escapeKey.wasPressedThisFrame;
        }

    }
}