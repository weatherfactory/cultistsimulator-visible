// Usage example of Google Universal Analytics.
//
// Copyright 2013-2019 Jetro Lauha (Strobotnik Ltd)
// http://strobotnik.com
// http://jet.ro
//
// $Revision: 1347 $
//
// File version history:
// 2013-09-01, 1.1.1 - Initial version
// 2013-09-25, 1.1.3 - Unity 3.5 support.
// 2013-12-17, 1.2.0 - Added user opt-out from analytics toggle.
// 2016-01-29, 1.6.2 - Moved user opt-out to main screen and social, exception
//                     and error hit examples to this screen.
//                     Use Analytics.getActiveSceneName() which uses Unity 5.3+
//                     SceneManager when available.
// 2017-04-07, 1.7.0 - Namespace support, minor changes.
// 2019-02-11, 1.8.0 - Minor changes.

using UnityEngine;
using System.Collections;
#if !UNITY_4_7 && !UNITY_4_6 && !UNITY_4_5 && !UNITY_4_3 && !UNITY_4_2 && !UNITY_4_1 && !UNITY_4_0 && !UNITY_3_5
using Strobotnik.GUA;
#endif

public class AnalyticsExampleSecondaryScreen : MonoBehaviour
{
    void OnGUI()
    {
        if (Analytics.gua == null)
        {
            // Error - AnalyticsExampleSecondaryScreen needs Analytics
            // object to be present which has been initialized by the
            // main AnalyticsExample scene.
            GUILayout.BeginVertical();
            GUILayout.Label("Error: No Analytics.gua object!\n");
            GUILayout.Label("AnalyticsExampleSecondaryScene works only when switched to from the main AnalyticsExample scene.");
            GUILayout.EndVertical();
            return;
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label(" ");
        GUILayout.BeginVertical();

        GUILayout.Label(" Current scene: " + Analytics.getActiveSceneName());
        GUILayout.Label(" ");
        GUILayout.Label(" This scene demonstrates automatic screen switch\n" +
                        " events sent by the analytics example, and is an\n" +
                        " example of social, error and exception hits.");

        GUILayout.Label("\nSocial hits and events - Links to Strobotnik:");
        // This is just an inspirational example. In reality you should
        // integrate official social SDKs and probably send the "Like"
        // type of analytics hit only when user actually does that
        // inside your application.
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Facebook"))
        {
            Analytics.gua.sendSocialHit("Facebook", "like", "StrobotnikFacebook");
            Application.OpenURL("http://facebook.com/strobotnik");
        }
        if (GUILayout.Button("Twitter"))
        {
            Analytics.gua.sendSocialHit("Twitter", "follow", "StrobotnikTwitter");
            Application.OpenURL("http://twitter.com/strobotnik");
        }
        if (GUILayout.Button(" Web "))
        {
            Analytics.gua.sendEventHit("OpenWebsite", "Strobotnik.com");
            Application.OpenURL("http://strobotnik.com");
        }
        GUILayout.EndHorizontal();

        GUILayout.Label("\nSend errors and exceptions to analytics:");
        if (Analytics.Instance.sendExceptions)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Log Error"))
            {
                Debug.LogError("Logged Error to analytics", this);
            }
            if (GUILayout.Button("Divide by zero"))
            {
                // Cause an exception to be sent to analytics
                int b = 0;
                int a = 31337 / b;
                Debug.Log("" + a); // won't get here
            }
            GUILayout.EndHorizontal();
        }
        else
            GUILayout.Label("(Analytics.sendExceptions is disabled)");

        GUILayout.Label("\nMore from Strobotnik:");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Pixel-Perfect\nDynamic Text"))
        {
            Analytics.gua.sendEventHit("OpenWebsite", "bitly.com/DynTextUnity");
            Application.OpenURL("http://bitly.com/DynTextUnity");
        }
        if (GUILayout.Button("Internet\nReachability\nVerifier"))
        {
            Analytics.gua.sendEventHit("OpenWebsite", "j.mp/IRVUNAS");
            Application.OpenURL("http://j.mp/IRVUNAS");
        }
        if (GUILayout.Button("Klattersynth\nTTS"))
        {
            Analytics.gua.sendEventHit("OpenWebsite", "j.mp/KLTTR");
            Application.OpenURL("http://j.mp/KLTTR");
        }
        GUILayout.EndHorizontal();

        GUILayout.Label("\n");
        if (GUILayout.Button("Back to Main"))
        {
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
            Application.LoadLevel("AnalyticsExample");
#else
            UnityEngine.SceneManagement.SceneManager.LoadScene("AnalyticsExample");
#endif
        }
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }
}
