using UnityEngine;
using System.Collections;
using Assets.CS.TabletopUI;
using UnityEngine.UI;

public class OptionsPanel : MonoBehaviour
{

    [SerializeField] private TabletopManager tabletopManager;
    [SerializeField] private BackgroundMusic backgroundMusic;
    [SerializeField] private Toggle musicToggle;

    public void ToggleVisibility()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    public void LoadGame()
    {
        tabletopManager.LoadGame();
        ToggleVisibility();
    }

    public void SaveGame()
    {
        tabletopManager.SaveGame();
        ToggleVisibility();
    }

    public void RestartGame()
    {
        tabletopManager.RestartGame();
    }

    public void ToggleMusic()
    {
        backgroundMusic.SetMute(!musicToggle.isOn);
        
    }
}
