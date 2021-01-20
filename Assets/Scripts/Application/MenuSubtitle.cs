
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.Services;
using SecretHistories.UI;

using TMPro;
using UnityEngine;

public class MenuSubtitle : MonoBehaviour
{

    [SerializeField] public TextMeshProUGUI SubtitleText;

    [SerializeField] public TextMeshProUGUI SubtitleTextShadow;


    public void Start()
    {

        var concursum = Watchman.Get<Concursum>();
        concursum.ContentUpdatedEvent.AddListener(OnContentUpdated);
        ShowSubtitle();

    }

    private void OnContentUpdated(ContentUpdatedArgs arg0)
    {
        ShowSubtitle();
    }

    public void ShowSubtitle()
    {
        //update subtitle text

        if (Watchman.Get<Character>().ActiveLegacy != null)
            //we need to go the long wway round because the label on the legacy entity in the character won't have changed if the compendium has just been repopulated with a different culture
            SetText(Watchman.Get<Compendium>().GetEntityById<Legacy>(Watchman.Get<Character>().ActiveLegacy.Id).Label);
        else
        {
            if (NoonUtility.PerpetualEdition)
            {
                UpdateWithLocValue("UI_PERPETUAL_EDITION");
            }
            else
            {
                UpdateWithLocValue("UI_BRING_THE_DAWN");

            }
        }
    }
    
    public void UpdateWithLocValue(string text)
    {
        string currentCultureId = Watchman.Get<Config>().GetConfigValue(NoonConstants.CULTURE_SETTING_KEY);

        var currentCulture = Watchman.Get<Compendium>().GetEntityById<Culture>(currentCultureId);


        Babelfish subfish = SubtitleText.gameObject.GetComponent<Babelfish>();
		if (subfish)
		{ 
			subfish.UpdateLocLabel(text); 
            subfish.SetValuesFromCulture(currentCulture);
        }

        Babelfish shadowfish = SubtitleTextShadow.gameObject.GetComponent<Babelfish>();
		if (shadowfish)
		{
            shadowfish.UpdateLocLabel(text);
            shadowfish.SetValuesFromCulture(currentCulture);
        }

    }


    public void SetText(string text)
    {
        SubtitleText.text = text;
        SubtitleTextShadow.text = text;
    }

}
