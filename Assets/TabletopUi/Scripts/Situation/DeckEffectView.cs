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

public class DeckEffectView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField] private Image deckBack;
    [SerializeField] private TextMeshProUGUI deckQuantity;

    private string deckId;
    private int quantity;

    public void PopulateDisplay(string deckId, int quantity) {
        this.deckId = deckId;
        this.quantity = quantity;

        deckBack.sprite = GetSpriteForDeck(deckId);

        deckQuantity.gameObject.SetActive(quantity > 1);
        deckQuantity.text = (quantity > 1 ? quantity.ToString() : null);

        gameObject.name = "DeckEffectView - " + deckId + " - " + quantity;
    }

    Sprite GetSpriteForDeck(string deckId) {
        // TODO: This is temp since I can't get from the string to the card back yet
        float random = UnityEngine.Random.value;

        if (random < 0.33f)
            return ResourcesManager.GetSpriteForCardBack("books");
        else if (random < 0.66f)
            return ResourcesManager.GetSpriteForCardBack("eye");
        else 
            return ResourcesManager.GetSpriteForCardBack("default");

        // Something along these lines instead
        //IGameEntityStorage storage;
        //var deck = storage.GetDeckInstanceById(deckId)
    }

    public void OnPointerEnter(PointerEventData eventData) {
        SoundManager.PlaySfx("TokenHover");
        deckBack.canvasRenderer.SetColor(UIStyle.aspectHover);
    }

    public void OnPointerExit(PointerEventData eventData) {
        deckBack.canvasRenderer.SetColor(Color.white);
    }

    public void OnPointerClick(PointerEventData eventData) {
        Registry.Retrieve<INotifier>().ShowDeckDetails(deckId, quantity);
    }


}
