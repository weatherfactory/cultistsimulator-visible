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
using SecretHistories.Abstract;
using SecretHistories.Entities;

public class ElementFrame : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int Quantity;
    private Element _aspect=null;
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

    public void PopulateDisplay(IManifestable manifestable)
    {
        //If we've passed a manifestable here, we probably want to cast it as an element and display that. But maybe we want to convert
        //it in a more complex way. This method leaves room for eg 'a verb but let's get a likely element from that'
        string potentialElementId = manifestable.Id;
        Element letsShowThisElementThen = Watchman.Get<Compendium>().GetEntityById<Element>(potentialElementId);
        //might be the null element, that's ok
        PopulateDisplay(letsShowThisElementThen,1);

    }
    

        public void PopulateDisplay(Element element, int quantity, bool hasBrightBg = false)
    {

        _aspect = element;
        Quantity = quantity;
        DisplayAImage(element);
        DisplayQuantity(quantity, hasBrightBg);
        gameObject.name = "Element - " + element.Id;
    }

    private void DisplayAImage(Element element)
    {
        Sprite aspectSprite;
        if (element.IsAspect) //it may be a concrete element rather than just an aspect
            aspectSprite= ResourcesManager.GetSpriteForAspect(element.Icon);
        else
            aspectSprite = ResourcesManager.GetSpriteForElement(element.Icon);

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
        //if(_elementStack!=null)
        //    Watchman.Get<INotifier>().ShowCardElementDetails(_aspect, _elementStack);
        //else
            Watchman.Get<Notifier>().ShowElementDetails(_aspect, parentIsDetailsWindow);
    }

    public Vector3 GetNotificationPosition()
    {
        Vector3 v3 = transform.position;
        v3.x += 130;
        return v3;
    }

}
