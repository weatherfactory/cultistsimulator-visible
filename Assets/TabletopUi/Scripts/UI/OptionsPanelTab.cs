using System.Globalization;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsPanelTab : MonoBehaviour
{
    [SerializeField] private Image TabImage;
    [SerializeField] private TMP_Text TabText;
    

    public void Initialise(string tabName)
    {
        CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
        TextInfo textInfo = cultureInfo.TextInfo;

        TabText.text = textInfo.ToTitleCase(tabName);

        TabImage.sprite=ResourcesManager.GetSprite("ui/tabs", tabName);
    }
}