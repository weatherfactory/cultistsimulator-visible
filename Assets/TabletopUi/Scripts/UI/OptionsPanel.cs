using UnityEngine;
using System.Collections;
using Assets.CS.TabletopUI;

public class OptionsPanel : MonoBehaviour
{

    [SerializeField] private TabletopManager tabletopManager;

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
}
