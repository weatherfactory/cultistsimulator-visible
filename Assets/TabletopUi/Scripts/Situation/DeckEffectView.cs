#pragma warning disable 0649
using UnityEngine;
using System.Collections;
using System.Linq;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using Assets.Core.Entities;

public class DeckEffectView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField] private Image deckBack;
    [SerializeField] private TextMeshProUGUI deckQuantity;

    private DeckSpec deckSpec;
    private int quantity;

    public void PopulateDisplay(DeckSpec deckSpeck, int quantity) {
        this.deckSpec = deckSpeck;
        this.quantity = quantity;

        deckBack.sprite = ResourcesManager.GetSpriteForCardBack(deckSpeck.Id); 

        deckQuantity.gameObject.SetActive(quantity > 1);
        deckQuantity.text = (quantity > 1 ? quantity.ToString() : null);

        gameObject.name = "DeckEffectView - " + deckSpeck + " - " + quantity;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        SoundManager.PlaySfx("TokenHover");
        deckBack.canvasRenderer.SetColor(UIStyle.aspectHover);
    }

    public void OnPointerExit(PointerEventData eventData) {
        deckBack.canvasRenderer.SetColor(Color.white);
    }

    public void OnPointerClick(PointerEventData eventData) {
        Registry.Get<INotifier>().ShowDeckDetails(deckSpec, quantity);
    }


}
