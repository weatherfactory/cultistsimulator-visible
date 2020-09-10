using System.Collections;
using System.Collections.Generic;
using Assets.CS.TabletopUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RestartButton : MonoBehaviour
{
#pragma warning disable 649
    [SerializeField] private TextMeshProUGUI ButtonText;
#pragma warning restore 649
    private bool clickedOnce = false;

    public void Awake()
    {
        OnLanguageChanged();
    }

    public bool AttemptRestart()
    {
        if (clickedOnce)
            return true;
        
        ButtonText.text = Registry.Get<ILocStringProvider>().Get( "UI_RESTARTSURE" );
        clickedOnce = true;
        StartCoroutine(BrieflyDisable());
        return false;
    }

    public void ResetState()
    {
        OnLanguageChanged();
        clickedOnce = false;
		gameObject.GetComponent<Button>().interactable = true;	// Re-enable button in case menu was closed during coroutine
    }

    public IEnumerator BrieflyDisable()
    {
        gameObject.GetComponent<Button>().interactable = false;
        yield return new WaitForSeconds(2);
        gameObject.GetComponent<Button>().interactable = true;
    }

	public virtual void OnLanguageChanged()
    {
        ButtonText.text = Registry.Get<ILocStringProvider>().Get( "UI_RESTART" );
    }

}
