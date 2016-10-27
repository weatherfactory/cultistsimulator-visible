using UnityEngine;
using System.Collections;
using TMPro;

public class EndingPanel : MonoBehaviour
{

    [SerializeField] Heartbeat heartbeat;
[SerializeField]public TextMeshProUGUI TitleText;
    [SerializeField] public TextMeshProUGUI DetailText;


    public void NewGame()
    {
        heartbeat.NewGame();
    }

    public void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        heartbeat.QuitApplication();
    }
}
