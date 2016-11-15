using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class AspectFrame : MonoBehaviour,IPointerClickHandler
{
    public int Quantity;
    private Element aspect=null;
    [SerializeField] private Image aspectImage=null;
    [SerializeField] private Text quantityText = null;
    public string AspectId { get { return aspect == null ? null : aspect.Id; } }

    private void DisplayAspectImage(Element aspect)
    {
        Sprite aspectSprite = ResourcesManager.GetSpriteForAspect(aspect.Id);
        aspectImage.sprite = aspectSprite;
    }

    private void DisplayQuantity(int quantity)
    {
        quantityText.text = quantity.ToString();

    }

    public void PopulateDisplay(Element aspect, int aspectValue)
    {
        Quantity = aspectValue;
        DisplayAspectImage(aspect);
        DisplayQuantity(aspectValue);
        gameObject.name = "Aspect - " + aspect.Id;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
    //    BM.Notify(Aspect.Label,Aspect.Description, gameObject.GetComponent<INotifyLocator>());
   }

    public Vector3 GetNotificationPosition()
    {
        Vector3 v3 = transform.position;
        v3.x += 130;
        return v3;
    }
}
