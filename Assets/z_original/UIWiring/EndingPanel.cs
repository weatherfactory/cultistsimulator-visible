using UnityEngine;
using System.Collections;
using TMPro;

public class EndingPanel : MonoBehaviour
{

    [SerializeField] Heart _heart;
[SerializeField]public TextMeshProUGUI TitleText;
    [SerializeField] public TextMeshProUGUI DetailText;


    public void NewGame()
    {
        _heart.NewGame();
    }

    public void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        _heart.QuitApplication();
    }
}
