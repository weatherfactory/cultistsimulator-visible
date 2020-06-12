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

public class ElementFrame : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int Quantity;
    private Element _aspect=null;
    private ElementStackToken _elementStackToken;
    private bool parentIsDetailsWindow = false; // set by AspectsDisplay. Used in Notifier call.

    [SerializeField] private LayoutElement layoutElement;
    [SerializeField] private Image aspectImage;
    [SerializeField] private TextMeshProUGUI quantityText;

    [SerializeField] Color brightQuantityColor;
    [SerializeField] Color darkQuantityColor;

    public string ElementId { get { return _aspect == null ? null : _aspect.Id; } }

	// Hardwired widths for aspect icons depending on number of digits. Tweaked to make them look neater when wrapping onto two rows - CP
    float width2Digit = 80f; //85f;
    float width1Digit = 60f; //68f;
    float width0Digits = 40f;

    public void PopulateDisplay(Element aspect, int aspectValue, ElementStackToken elementStackToken, bool hasBrightBg = false) {
        
        _aspect = aspect;
        _elementStackToken = elementStackToken; //this is populated only if the frame reflects a card that's being displayed in a situation storage
        Quantity = aspectValue;
        DisplayAspectImage(aspect);
        DisplayQuantity(aspectValue, hasBrightBg);
        gameObject.name = "Aspect - " + aspect.Id;
    }

    private void DisplayAspectImage(Element aspect)
    {
        Sprite aspectSprite;
        if (aspect.IsAspect) //it may be a concrete element rather than just an aspect
            aspectSprite= ResourcesManager.GetSpriteForAspect(aspect.Icon);
        else
            aspectSprite = ResourcesManager.GetSpriteForElement(aspect.Icon);

        aspectImage.sprite = aspectSprite;
    }

    private void DisplayQuantity(int quantity, bool hasBrightBG) {
        if (quantity <= 1) {
            quantityText.gameObject.SetActive(false);
            layoutElement.minWidth = width0Digits;
            layoutElement.preferredWidth = width0Digits;
        }
        else {
            quantityText.gameObject.SetActive(true);
            quantityText.text = quantity.ToString();
            quantityText.color = hasBrightBG ? darkQuantityColor : brightQuantityColor;
            layoutElement.minWidth = quantity > 9 ? width2Digit : width1Digit; ;
            layoutElement.preferredWidth = layoutElement.minWidth;
        }
    }

    public void SetAsDetailWindowChild() {
        parentIsDetailsWindow = true;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        SoundManager.PlaySfx("TokenHover");
        aspectImage.canvasRenderer.SetColor(UIStyle.aspectHover);
    }

    public void OnPointerExit(PointerEventData eventData) {
        aspectImage.canvasRenderer.SetColor(Color.white);
    }

    public void OnPointerClick(PointerEventData eventData) {
        if(_elementStackToken!=null)
            Registry.Retrieve<INotifier>().ShowCardElementDetails(_aspect, _elementStackToken);
        else
            Registry.Retrieve<INotifier>().ShowElementDetails(_aspect, parentIsDetailsWindow);
    }

    public Vector3 GetNotificationPosition()
    {
        Vector3 v3 = transform.position;
        v3.x += 130;
        return v3;
    }

}
