using System.Globalization;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsPanelTab : MonoBehaviour
{
    [SerializeField] private Image TabImage;
    [SerializeField] private TMP_Text TabText;
    
    public string TabId { get; private set; }
    public bool Selected { get; private set; }

    public void Initialise(string tabId)
    {
        TabId = tabId;
        
        gameObject.name = "Tab_" + tabId;
        CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
        TextInfo textInfo = cultureInfo.TextInfo;

        TabText.text = textInfo.ToTitleCase(tabId);

        TabImage.sprite=ResourcesManager.GetSprite("ui/tabs", tabId);
    }

    public void Select()
    {
        Selected = true;
    }

    public void Deselect()
    {
        Selected = false;
    }
}