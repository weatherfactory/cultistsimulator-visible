using UnityEngine;
using System.Collections;
using Assets.CS.TabletopUI;
using UnityEngine.UI;

public class OptionsPanel : MonoBehaviour
{

    [SerializeField] private BackgroundMusic backgroundMusic;
    [SerializeField] private Toggle musicToggle;

    public void ToggleVisibility()
    {
        gameObject.SetActive(!gameObject.activeSelf);

        Registry.Retrieve<TabletopManager>().SetPausedState(gameObject.activeInHierarchy);

    }


    public void RestartGame()
    {

        Registry.Retrieve<TabletopManager>().RestartGame();
        ToggleVisibility();
    }
    public void LeaveGame()
    {

  Application.Quit();
    }

    public void ToggleMusic()
    {
        backgroundMusic.SetMute(!musicToggle.isOn);
        
    }
}
