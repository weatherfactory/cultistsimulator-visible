using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public class SpeedController:MonoBehaviour
    {
        [SerializeField] private PauseButton pauseButton;
        [SerializeField] private Button normalSpeedButton;
        [SerializeField] private Button fastForwardButton;


        private readonly Color activeSpeedColor = new Color32(147, 225, 239, 255);
        private readonly Color inactiveSpeedColor = Color.white;
        private Heart _heart;

        public void Initialise(Heart heart)
        {
            normalSpeedButton.GetComponent<Image>().color = inactiveSpeedColor;
            _heart = heart;
        }

        public void SetPausedState(bool pause)
        {
            if (pause)
            {
                _heart.StopBeating();
                pauseButton.SetPausedText(true);
                pauseButton.GetComponent<Image>().color = activeSpeedColor;
                normalSpeedButton.GetComponent<Image>().color = inactiveSpeedColor;
                fastForwardButton.GetComponent<Image>().color = inactiveSpeedColor;

            }
            else
            {
                _heart.ResumeBeating();
                pauseButton.SetPausedText(false);
                pauseButton.GetComponent<Image>().color = inactiveSpeedColor;
                if (_heart.GetGameSpeed() == GameSpeed.Fast)
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

        public void TogglePause()
        {
            SetPausedState(!_heart.IsPaused);
        }

        public void SetNormalSpeed()
        {
            if (_heart.IsPaused)
                SetPausedState(false);
            _heart.SetGameSpeed(GameSpeed.Normal);
            normalSpeedButton.GetComponent<Image>().color = activeSpeedColor;
            fastForwardButton.GetComponent<Image>().color = inactiveSpeedColor;


        }

        public void SetFastForward()
        {

            if (_heart.IsPaused)
                SetPausedState(false);
            _heart.SetGameSpeed(GameSpeed.Fast);

            normalSpeedButton.GetComponent<Image>().color = inactiveSpeedColor;
            fastForwardButton.GetComponent<Image>().color = activeSpeedColor;

        }

    }
}
