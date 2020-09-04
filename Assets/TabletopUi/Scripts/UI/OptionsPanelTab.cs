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
    private OptionsPanel _parentOptionsPanel;

    public void Initialise(string tabId,OptionsPanel parentOptionsPanel)
    {
        TabId = tabId;
        _parentOptionsPanel = parentOptionsPanel;
        TabText.text = tabId;
        TabImage.sprite=ResourcesManager.GetSprite("ui/tabs", tabId);
    }

    public void Activate()
    {
       gameObject.GetComponent<Button>().Select();
       //without the next line, Unity doesn't highlight the selected buttton, because it's hung up on it already being selected, or something
       gameObject.GetComponent<Button>().OnSelect(null);
      _parentOptionsPanel.TabActivated(this);
      TabImage.color = new Color(1, 1, 1, 1);

    }

    public void Deactivate()
    {
       //
    }
}