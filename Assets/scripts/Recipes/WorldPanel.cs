using UnityEngine;
using System.Collections;

public class WorldPanel : MonoBehaviour {

    [SerializeField]
    private GameObject prefabTimerPanel;

    public void AddTimer(Recipe forRecipe)
    {
        GameObject newTimerPanelGameObject = Instantiate(prefabTimerPanel, transform) as GameObject;
        TimerPanel newTimerPanel = newTimerPanelGameObject.GetComponent<TimerPanel>();
        newTimerPanel.StartTimer(forRecipe);
    }
}
