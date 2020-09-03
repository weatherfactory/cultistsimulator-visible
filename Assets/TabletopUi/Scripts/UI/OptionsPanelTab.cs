using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsPanelTab : MonoBehaviour
{
    [SerializeField] private Image TabImage;
    [SerializeField] private TMP_Text TabText;
    

    public void Initialise(string tabName)
    {
        TabText.text = tabName;
    }
}