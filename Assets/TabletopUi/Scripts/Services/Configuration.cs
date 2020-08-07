using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Services
{
   public class Configuration
    {

        public static void Setup()
        {
#if UNITY_STANDALONE_OSX
            // Vsync doesn't seem to limit the FPS on the mac so well, so we set it to 0 and force a target framerate (setting it to 0 any other way doesn't work, has to be done in code, apparently in Start not Awake too) - FM
            QualitySettings.vSyncCount = 0;
#else
            QualitySettings.vSyncCount = 1; // Force VSync on in case user has tried to disable it. No benefit, just burns CPU - CP
#endif

            SetFrameRateForCurrentGraphicsLevel();

        }

        public static void SetGraphicsLevel(int level)
        {
            QualitySettings.SetQualityLevel(level);
            SetFrameRateForCurrentGraphicsLevel();

        }

        public static void SetFrameRateForCurrentGraphicsLevel()
        {
            int currentLevel = QualitySettings.GetQualityLevel();
            if (currentLevel > 1)
                Application.targetFrameRate = 60;
            else
                Application.targetFrameRate = 30; //ram down the frame rate for v low quality
        }

        public static void SetWindowed(bool windowed)
        {
            if (windowed)

                Screen.SetResolution(Screen.width, Screen.height, false);
            else
                Screen.SetResolution(Screen.width, Screen.height, true);

        }

        public static void SetResolution(Resolution resolution)
        {
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }
    }
}
