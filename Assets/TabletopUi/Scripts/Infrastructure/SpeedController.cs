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
        private Heart _heart;

        public void Initialise(Heart heart)
        {
            normalSpeedButton.GetComponent<Image>().color = inactiveSpeedColor;
            _heart = heart;
        }

        public void LockToPause(bool locked) {
            isLocked = locked;

            if (locked) { 
                lastPauseState = _heart.IsPaused;
                SetPausedState(true);
            }
            else {
                SetPausedState(lastPauseState);
            }

			// We lock, that means we also don't want the player moving the table.
			scrollRectMover.enabled = !isLocked;
        }

        public bool GetPausedState()
        {
			return _heart.IsPaused;
		}

		public void SetPausedState(bool pause, bool withSFX = true, bool withFlash = false)
        {
            if (_heart.IsPaused == pause)
            {
                // No SFX or flash if no change of state - CP
                withSFX = false;
                withFlash = false;
            }

            if (pause || isLocked)
            {
                _heart.StopBeating();
                pauseButton.SetPausedText(true);
                pauseButton.GetComponent<Image>().color = activeSpeedColor;
                normalSpeedButton.GetComponent<Image>().color = inactiveSpeedColor;
                fastForwardButton.GetComponent<Image>().color = inactiveSpeedColor;

				if (withSFX)
					SoundManager.PlaySfx("UIPauseStart");

                if (withFlash)
                    pauseButton.RunFlashAnimation();
            }
            else
			{
				if (withSFX)
					SoundManager.PlaySfx("UIPauseEnd");
				
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

        public void TogglePause() {
            if (isLocked)
                return;

            SetPausedState(!_heart.IsPaused);
        }

        public void SetNormalSpeed() {
            if (isLocked)
                return;
            if (_heart.IsPaused)
                SetPausedState(false);

            _heart.SetGameSpeed(GameSpeed.Normal);
            normalSpeedButton.GetComponent<Image>().color = activeSpeedColor;
            fastForwardButton.GetComponent<Image>().color = inactiveSpeedColor;
        }

        public void SetFastForward() {
            if (isLocked)
                return;
            if (_heart.IsPaused)
                SetPausedState(false);

            _heart.SetGameSpeed(GameSpeed.Fast);
            normalSpeedButton.GetComponent<Image>().color = inactiveSpeedColor;
            fastForwardButton.GetComponent<Image>().color = activeSpeedColor;
        }

    }
}
