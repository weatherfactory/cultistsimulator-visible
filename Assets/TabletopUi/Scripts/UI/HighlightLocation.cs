using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HighlightLocation : MonoBehaviour
{

    [SerializeField]
    private Image _sprite;
    [SerializeField]
    private TMP_Text _label;
    [SerializeField] public string MatchElementId;

    public void Activate(float duration=1f)
    {

        SoundManager.PlaySfx("HighlightLocation");

      _sprite.gameObject.SetActive(true);
      _sprite.canvasRenderer.SetAlpha(0f);
      _sprite.CrossFadeAlpha(1f, duration, true);

      _label.gameObject.SetActive(true);
      _label.canvasRenderer.SetAlpha(0f);
      _label.CrossFadeAlpha(1f, duration, true);
    }

    public void Deactivate(float duration = 1f)
    {
        _sprite.gameObject.SetActive(false);
        _sprite.canvasRenderer.SetAlpha(1f);
        _sprite.CrossFadeAlpha(0f, duration, true);

        _label.gameObject.SetActive(false);
        _label.canvasRenderer.SetAlpha(1f);
        _label.CrossFadeAlpha(0f, duration, true);

    }
}
