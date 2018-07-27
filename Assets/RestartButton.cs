using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RestartButton : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI ButtonText;

    private bool clickedOnce = true;

    public void Awake()
    {
        OnLanguageChanged();
    }

    public bool AttemptRestart()
    {
        if (clickedOnce)
            return true;
        
        ButtonText.text = LanguageTable.Get( "UI_RESTARTSURE" );
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
        ButtonText.text = LanguageTable.Get( "UI_RESTART" );
    }

}
