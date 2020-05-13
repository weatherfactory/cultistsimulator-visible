using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HighlightLocation : MonoBehaviour
{

    [SerializeField]
    private Image _glowMarker;
    [SerializeField]
    private Image _label;
    [SerializeField]
    private Image _domicile;

    private bool _displayingForPresence = false;
    private bool _highlightingForInteraction = false;
    /// <summary>
    /// Highlight while element is interacted with
    /// </summary>
    [SerializeField] public string HighlightWhileElementIdInteracting;
    /// <summary>
    /// Display, including additional domicile marker, when this element is present
    /// </summary>
    [SerializeField] public string DisplayWhileElementIdPresent;


    public void HighlightForInteracted(float duration=1f)
    {
        _highlightingForInteraction = true;

        SoundManager.PlaySfx("HighlightLocation");

      _glowMarker.gameObject.SetActive(true);
      _glowMarker.canvasRenderer.SetAlpha(0f);
      _glowMarker.CrossFadeAlpha(1f, duration, true);

        if(!_displayingForPresence)
        {
            _label.gameObject.SetActive(true);
      _label.canvasRenderer.SetAlpha(0f);
      _label.CrossFadeAlpha(1f, duration, true);
        }
    }

    public void HideForNoInteraction()
    {
        if(_highlightingForInteraction)
        {
            _highlightingForInteraction = false;
            float duration = 0.5f;
            
            _glowMarker.gameObject.SetActive(false);
            _glowMarker.canvasRenderer.SetAlpha(1f);
            _glowMarker.CrossFadeAlpha(0f, duration, true);

            if (!_displayingForPresence)
            {
                _label.gameObject.SetActive(false);
                _label.canvasRenderer.SetAlpha(1f);
                _label.CrossFadeAlpha(0f, duration,true);
            }

        }

    }

    public void DisplayForPresence(float duration = 1f)
    {
        _displayingForPresence = true;

        _label.gameObject.SetActive(true);
        _label.canvasRenderer.SetAlpha(0f);
        _label.CrossFadeAlpha(1f, duration, true);

        _domicile.gameObject.SetActive(true);
        _domicile.canvasRenderer.SetAlpha(0f);
        _domicile.CrossFadeAlpha(1f, duration, true);
    }

    public void HideForNoPresence()
    {
        if (_displayingForPresence)
        {
            _displayingForPresence = false;

            _domicile.gameObject.SetActive(false);
            _domicile.canvasRenderer.SetAlpha(1f);
            _domicile.CrossFadeAlpha(0f, 0f, true);

            if (!_highlightingForInteraction)
            {
                _label.gameObject.SetActive(false);
                _label.canvasRenderer.SetAlpha(1f);
                _label.CrossFadeAlpha(0f, 0f, true);
            }
        }
    }



    public void HideCompletely(float duration = 1f)
    {
        _displayingForPresence = false;
        _highlightingForInteraction = false;

        _glowMarker.gameObject.SetActive(false);
        _glowMarker.canvasRenderer.SetAlpha(1f);
        _glowMarker.CrossFadeAlpha(0f, duration, true);

        _label.gameObject.SetActive(false);
        _label.canvasRenderer.SetAlpha(1f);
        _label.CrossFadeAlpha(0f, duration, true);

        _domicile.gameObject.SetActive(false);
        _domicile.canvasRenderer.SetAlpha(1f);
        _domicile.CrossFadeAlpha(0f, duration, true);
    }


}
