using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public class SpeedController:MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private PauseButton pauseButton;
        [SerializeField] private Button normalSpeedButton;
        [SerializeField] private Button fastForwardButton;
		[SerializeField] private ScrollRectMouseMover scrollRectMover;
#pragma warning restore 649
        private bool isLocked = false;
        private bool lastPauseState;
        private readonly Color activeSpeedColor = new Color32(147, 225, 239, 255);
        private readonly Color inactiveSpeedColor = Color.white;
        
        private GameSpeed[] PauseState;


        public void Start()
        {
            normalSpeedButton.GetComponent<Image>().color = inactiveSpeedColor;
            PauseState=new GameSpeed[3];

            }


        public void AttractAttention()
        {
            pauseButton.RunFlashAnimation(inactiveSpeedColor);
        }

        public void SetPausedState(int CommandPriority, bool pause, bool withSFX = true)
        {
            //if (_heart.IsPaused == pause)
            //{
            //    // No SFX or flash if no change of state - CP
            //    withSFX = false;
            //}

            if (pause || isLocked)
            {
           //     _heart.StopBeating();
                pauseButton.SetPausedText(true);
                pauseButton.SetColor(activeSpeedColor);
                normalSpeedButton.GetComponent<Image>().color = inactiveSpeedColor;
                fastForwardButton.GetComponent<Image>().color = inactiveSpeedColor;

				if (withSFX)
					SoundManager.PlaySfx("UIPauseStart");

            }
            else
			{
				if (withSFX)
					SoundManager.PlaySfx("UIPauseEnd");
				
            //    _heart.ResumeBeating();
                pauseButton.SetPausedText(false);
                pauseButton.SetColor(inactiveSpeedColor);

                if (PauseState[0] == GameSpeed.Fast)
                {
                    normalSpeedButton.GetComponent<Image>().color = inactiveSpeedColor;
                    fastForwardButton.GetComponent<Image>().color = activeSpeedColor;
                }
                else
                {
                    normalSpeedButton.GetComponent<Image>().color = activeSpeedColor;
                    fastForwardButton.GetComponent<Image>().color = inactiveSpeedColor;
                }
            }
        }

        public void TogglePause() {


            SetPausedState(1,PauseState[1]==GameSpeed.Paused);
        }

        public void SetSpeed(GameSpeed speedToSet)
        {
   
            if (PauseState[0] == GameSpeed.Paused)
                SetPausedState(0,false);

            if(speedToSet==GameSpeed.Normal)
            {

               PauseState[0]= GameSpeed.Normal;
                normalSpeedButton.GetComponent<Image>().color = activeSpeedColor;
                fastForwardButton.GetComponent<Image>().color = inactiveSpeedColor;
            }
            else if(speedToSet==GameSpeed.Fast)
            {

                PauseState[0]=GameSpeed.Fast;
                normalSpeedButton.GetComponent<Image>().color = inactiveSpeedColor;
                fastForwardButton.GetComponent<Image>().color = activeSpeedColor;
            }
        }



    }
}
