#pragma warning disable 0649
using UnityEngine;
using System.Collections;
using OrbCreationExtensions;
using TMPro;
using UnityEngine.UI;
using Assets.CS.TabletopUI;

public class PauseButton : MonoBehaviour
{
	[SerializeField] Image buttonImage;
    [SerializeField] TextMeshProUGUI buttonText;

    private const double FLASH_PERIOD = 0.15;
    private const double FLASH_DURATION = 0.9;
    private bool _isFlashing;
    private double _timeSpent;
    private Color _originalColor;
    private Color _flashColor;

    private void OnEnable()
    {
        // subscribe to event for language change
        LanguageManager.LanguageChanged += OnLanguageChanged;
        _isFlashing = false;
    }

    private void OnDisable()
    {
        LanguageManager.LanguageChanged -= OnLanguageChanged;
        _isFlashing = false;
    }

    public void OnLanguageChanged()
    {
		// Temp removed because we've settled on language switching ONLY in front end, so this is redundant.
		//var tabletop = Registry.Retrieve<ITabletopManager>();
		//SetPausedText( tabletop.IsPaused() );
	}

    public void SetPausedText(bool isPaused)
    {
        if (isPaused)
        {
			//ButtonText.text = "Unpause <size=60%><alpha=#99>[SPACE]";
			buttonText.text = LanguageManager.Get("UI_UNPAUSE");
        }
        else
        {
			//ButtonText.text = "Pause <size=60%><alpha=#99>[SPACE]";
			buttonText.text = LanguageManager.Get("UI_PAUSE");
        }
    }

    public void SetColor(Color color)
    {
	    buttonImage.color = color;
    }

    public void RunFlashAnimation(Color flashColor)
    {
	    _timeSpent = 0.0;
	    _originalColor = buttonImage.color;
	    _flashColor = flashColor;
	    _isFlashing = true;
    }

    void Update()
    {
	    if (!_isFlashing) 
		    return;
	    
	    _timeSpent += Time.deltaTime;
	    int currentPeriod = (int) (_timeSpent / FLASH_PERIOD);
	    buttonImage.color = currentPeriod % 2 == 1 ? _flashColor : _originalColor;
	    
	    if (!(_timeSpent > FLASH_DURATION)) 
		    return;
	    _isFlashing = false;
	    _timeSpent = 0.0;
	    buttonImage.color = _originalColor;
    }

}
