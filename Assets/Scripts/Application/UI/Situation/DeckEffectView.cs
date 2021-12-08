#pragma warning disable 0649
using UnityEngine;
using System.Collections;
using System.Linq;
using SecretHistories.Fucine;
using SecretHistories.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using SecretHistories.Assets.Scripts.Application.Entities;
using SecretHistories.Entities;

public class DeckEffectView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField] private Image deckBack;
    [SerializeField] private TextMeshProUGUI deckQuantity;

    private DeckEffect _deckEffect;
    

    public void PopulateDisplay(DeckEffect deckEffect)
    {

        _deckEffect = deckEffect;
        deckBack.sprite = ResourcesManager.GetSpriteForCardBack(_deckEffect.DeckSpec.Id); 

        deckQuantity.gameObject.SetActive(deckEffect.Draws > 1);
        deckQuantity.text = (deckEffect.Draws > 1 ? deckEffect.Draws.ToString() : null);

        gameObject.name = "DeckEffectView - " + deckEffect.DeckSpec.Id + " - " + deckEffect.Draws;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        SoundManager.PlaySfx("TokenHover");
        deckBack.canvasRenderer.SetColor(UIStyle.aspectHover);
    }

    public void OnPointerExit(PointerEventData eventData) {
        deckBack.canvasRenderer.SetColor(Color.white);
    }

    public void OnPointerClick(PointerEventData eventData) {
        Watchman.Get<Notifier>().ShowDeckEffectDetails(_deckEffect);
    }


}
