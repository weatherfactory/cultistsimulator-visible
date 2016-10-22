using UnityEngine;
using System.Collections;
using System.Linq;
using Assets.scripts.Infrastructure;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AspectFrame : BoardMonoBehaviour,IPointerClickHandler,INotifyLocator
{
    public int Quantity;
    public Element Aspect;

    public void DisplayAspectImage(string aspectId)
    {
        Image aspectImage = GetComponentsInChildren<Image>().Single(i => i.name == "imgAspectIcon");
        Sprite aspectSprite = Resources.Load<Sprite>("icons40/aspects/" + aspectId);
        aspectImage.sprite = aspectSprite;
    }

    public void DisplayQuantity(int quantity)
    {
        Text quantityText = GetComponentsInChildren<Text>().Single(t => t.name == "txtQuantity");
        quantityText.text = quantity.ToString();

    }

    public void PopulateDisplay(string aspectId, int aspectValue, ContentRepository cr)
    {
        Aspect = cr.GetElementById(aspectId);
        Quantity = aspectValue;
        DisplayAspectImage(aspectId);
        DisplayQuantity(aspectValue);
        gameObject.name = "Aspect - " +aspectId;

    }

    public void ModifyQuantity(int quantityChange)
    {
       DisplayQuantity(Quantity+quantityChange);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        BM.Notify(Aspect.Label,Aspect.Description, gameObject.GetComponent<INotifyLocator>());
   }

    public Vector3 GetNotificationPosition()
    {
        Vector3 v3 = transform.position;
        v3.x += 130;
        return v3;
    }
}
