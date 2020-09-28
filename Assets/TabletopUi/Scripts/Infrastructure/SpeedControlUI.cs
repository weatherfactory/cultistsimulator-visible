using System;
using System.Linq;
using System.Text;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Interfaces;
using Noon;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public class SpeedControlUI:MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private PauseButton pauseButton;
        [SerializeField] private Button normalSpeedButton;
        [SerializeField] private Button fastForwardButton;

#pragma warning restore 649
        
        private readonly Color activeSpeedColor = new Color32(147, 225, 239, 255);
        private readonly Color inactiveSpeedColor = Color.white;

        private GameSpeedState uiShowsGameSpeed = new GameSpeedState();


        public void Start()
        {
            normalSpeedButton.GetComponent<Image>().color = inactiveSpeedColor;
            }


        public void AttractAttention()
        {
            pauseButton.RunFlashAnimation(inactiveSpeedColor);
        }

        public void PauseButton_OnClick()
        {
            Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 2, GameSpeed = GameSpeed.Paused, WithSFX = true });
        }

        public void NormalSpeedButton_OnClick()
        {
            Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs{ControlPriorityLevel = 1,GameSpeed = GameSpeed.Normal,WithSFX = false });
            Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 2, GameSpeed = GameSpeed.DeferToNextLowestCommand, WithSFX = false });
        }

        public void FastSpeedButtonOnClick()
        {
            Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 1, GameSpeed = GameSpeed.Fast, WithSFX = false });
            Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 2, GameSpeed = GameSpeed.DeferToNextLowestCommand, WithSFX = false });
        }

        
        public void RespondToSpeedControlCommand(SpeedControlEventArgs args)
        {
            
            uiShowsGameSpeed.SetGameSpeedCommand(args.ControlPriorityLevel,args.GameSpeed);
            

            if (uiShowsGameSpeed.GetEffectiveGameSpeed() == GameSpeed.Paused)
            {
                pauseButton.SetPausedText(true);
                pauseButton.SetColor(activeSpeedColor);
                normalSpeedButton.GetComponent<Image>().color = inactiveSpeedColor;
                fastForwardButton.GetComponent<Image>().color = inactiveSpeedColor;
            }
            else 
            {
                pauseButton.SetPausedText(false);
                pauseButton.SetColor(inactiveSpeedColor);

                if (uiShowsGameSpeed.GetEffectiveGameSpeed() == GameSpeed.Fast)
                {
                    normalSpeedButton.GetComponent<Image>().color = inactiveSpeedColor;
                    fastForwardButton.GetComponent<Image>().color = activeSpeedColor;
                }
                else if(uiShowsGameSpeed.GetEffectiveGameSpeed() == GameSpeed.Normal)
                {
                    normalSpeedButton.GetComponent<Image>().color = activeSpeedColor;
                    fastForwardButton.GetComponent<Image>().color = inactiveSpeedColor;
                }
                else
                {
                    NoonUtility.Log("Unknown effective game speed: " + uiShowsGameSpeed.GetEffectiveGameSpeed());
                }
            }

        }


    }
}
