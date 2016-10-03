using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ElementSlot : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    private string elementId;
    public string ElementId { get { return elementId; } }
    public string Description { get; set; }

    public void SetElementValues(string idToSet, ContentManager cm)
    {
        elementId = idToSet;
        SetName(idToSet);
        RetrieveIcon(idToSet);
        Description = cm.GetDescriptionForElementId(idToSet);
    }

    private void SetName(string idToSet)
    {
        Text nameText = GetComponentsInChildren<Text>()[0];
        nameText.text = idToSet;
    }

    private void RetrieveIcon(string idToSet)
    {
        Image[] childImages = GetComponentsInChildren<Image>();
        Image elementImage = childImages[2];
        Sprite elementSprite = Resources.Load<Sprite>("FlatIcons/png/32px/" + idToSet);
        elementImage.sprite = elementSprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("entered" + elementId);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("exited" + elementId);
    }
}
