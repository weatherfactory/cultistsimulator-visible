using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuSubtitle : MonoBehaviour
{

    [SerializeField] public TextMeshProUGUI SubtitleText;

    [SerializeField] public TextMeshProUGUI SubtitleTextShadow;

    public void SetText(string text)
    {
		Babelfish fish = SubtitleText.gameObject.GetComponent<Babelfish>();
		if (fish)
		{
			fish.SetLocLabel( text );
		}
		fish = SubtitleTextShadow.gameObject.GetComponent<Babelfish>();
		if (fish)
		{
			fish.SetLocLabel( text );
		}

        SubtitleText.text		= LanguageTable.Get(text);
        SubtitleTextShadow.text = LanguageTable.Get(text);
    }


}
