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
    }

    public void SaveGame()
    {
        tabletopManager.SaveGame();
    }

    public void RestartGame()
    {
        tabletopManager.ClearBoard();
    }
}
