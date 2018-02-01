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

public class AspectFrame : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int Quantity;
    private Element aspect=null;
    private bool parentIsDetailsWindow = false; // set by AspectsDisplay. Used in Notifier call.

    [SerializeField] private LayoutElement layoutElement;
    [SerializeField] private Image aspectImage;
    [SerializeField] private Image quantityBG;
    [SerializeField] private TextMeshProUGUI quantityText;
    public string AspectId { get { return aspect == null ? null : aspect.Id; } }

    public float widthWithQuantity = 80f;
    public float widthWithoutQuantity = 40f;

    public void PopulateDisplay(Element aspect, int aspectValue) {
        this.aspect = aspect;
        Quantity = aspectValue;
        DisplayAspectImage(aspect);
        DisplayQuantity(aspectValue);
        gameObject.name = "Aspect - " + aspect.Id;
    }

    private void DisplayAspectImage(Element aspect)
    {
        Sprite aspectSprite;
        if (aspect.IsAspect) //it may be a concrete element rather than just an aspect
         aspectSprite= ResourcesManager.GetSpriteForAspect(aspect.Id);
        else
            aspectSprite = ResourcesManager.GetSpriteForElement(aspect.Id);

        aspectImage.sprite = aspectSprite;
    }

    private void DisplayQuantity(int quantity) {
        if (quantity <= 1) {
            quantityBG.gameObject.SetActive(false);
            quantityText.gameObject.SetActive(false);
            layoutElement.minWidth = widthWithoutQuantity;
            layoutElement.preferredWidth = widthWithoutQuantity;
        }
        else {
            quantityBG.gameObject.SetActive(true);
            quantityText.gameObject.SetActive(true);
            quantityText.text = quantity.ToString();
            layoutElement.minWidth = widthWithQuantity;
            layoutElement.preferredWidth = widthWithQuantity;
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
        Registry.Retrieve<INotifier>().ShowElementDetails(aspect, parentIsDetailsWindow);
    }

    public Vector3 GetNotificationPosition()
    {
        Vector3 v3 = transform.position;
        v3.x += 130;
        return v3;
    }

}
