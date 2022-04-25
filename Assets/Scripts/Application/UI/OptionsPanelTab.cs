using System.Globalization;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class OptionsPanelTab : MonoBehaviour
{
#pragma warning disable 649
    [SerializeField] private Image TabImage;
    [SerializeField] private TMP_Text TabText;
#pragma warning restore 649

    public string TabId { get; private set; }
    private OptionsPanel _parentOptionsPanel;

    public void Initialise(string tabId,OptionsPanel parentOptionsPanel)
    {
        TabId = tabId;
        _parentOptionsPanel = parentOptionsPanel; TabText.GetComponent<Babelfish>().UpdateLocLabel(tabId);
        string iconRelativePath = Path.Combine("tabs", tabId);
        TabImage.sprite = ResourcesManager.GetSpriteForUI(iconRelativePath);
    }

    public void Activate()
    {
       gameObject.GetComponent<Button>().Select();
       //without the next line, Unity doesn't highlight the selected buttton, because it's hung up on it already being selected, or something
       gameObject.GetComponent<Button>().OnSelect(null);
      _parentOptionsPanel.TabActivated(this);
      TabImage.color = new Color(1, 1, 1, 1);
      TabText.color = new Color(1, 1, 1, 1);

    }

    public void Deactivate()
    {
        TabImage.color = new Color(1, 1, 1, 0.5f);
        TabText.color = new Color(1, 1, 1, 0.5f);
    }
}