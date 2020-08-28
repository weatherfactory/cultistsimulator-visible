using System;
using System.Linq;
using System.Text;
using Assets.CS.TabletopUI;
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
		[SerializeField] private ScrollRectMouseMover scrollRectMover;
#pragma warning restore 649
        
        private readonly Color activeSpeedColor = new Color32(147, 225, 239, 255);
        private readonly Color inactiveSpeedColor = Color.white;

        private GameSpeedState gameSpeedState = new GameSpeedState();


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
            Registry.Get<TabletopManager>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 2, GameSpeed = GameSpeed.Paused, WithSFX = true });
        }

        public void NormalSpeedButton_OnClick()
        {
            Registry.Get<TabletopManager>().SpeedControlEvent.Invoke(new SpeedControlEventArgs{ControlPriorityLevel = 1,GameSpeed = GameSpeed.Normal,WithSFX = true});
            Registry.Get<TabletopManager>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 2, GameSpeed = GameSpeed.Unspecified, WithSFX = false });

        }

        public void FastSpeedButtonOnClick()
        {
            Registry.Get<TabletopManager>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 1, GameSpeed = GameSpeed.Fast, WithSFX = true });
            Registry.Get<TabletopManager>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 2, GameSpeed = GameSpeed.Unspecified, WithSFX = false });
        }

        public void RespondToSpeedControlCommand(SpeedControlEventArgs args)
        {
            if(args.WithSFX)
                SoundManager.PlaySfx("UIPauseStart");
            else
                SoundManager.PlaySfx("UIPauseEnd");


            gameSpeedState.SetGameSpeedCommand(args.ControlPriorityLevel,args.GameSpeed);



            if (gameSpeedState.GetEffectiveGameSpeed() == GameSpeed.Paused)
            {
                //     _heart.StopBeating();
                pauseButton.SetPausedText(true);
                pauseButton.SetColor(activeSpeedColor);
                normalSpeedButton.GetComponent<Image>().color = inactiveSpeedColor;
                fastForwardButton.GetComponent<Image>().color = inactiveSpeedColor;
            }
            else 
            {
                //    _heart.ResumeBeating();
                pauseButton.SetPausedText(false);
                pauseButton.SetColor(inactiveSpeedColor);

                if (gameSpeedState.GetEffectiveGameSpeed() == GameSpeed.Fast)
                {
                    normalSpeedButton.GetComponent<Image>().color = inactiveSpeedColor;
                    fastForwardButton.GetComponent<Image>().color = activeSpeedColor;
                }
                else if(gameSpeedState.GetEffectiveGameSpeed() == GameSpeed.Normal)
                {
                    normalSpeedButton.GetComponent<Image>().color = activeSpeedColor;
                    fastForwardButton.GetComponent<Image>().color = inactiveSpeedColor;
                }
                else
                {
                    NoonUtility.Log("Unknown effective game speed: " + gameSpeedState.GetEffectiveGameSpeed());
                }
            }
        }


    }
}
