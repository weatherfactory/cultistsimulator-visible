using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.UI;

public class AspectFrame : MonoBehaviour
{
    public string AspectId;
    public int Quantity;


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

    public void PopulateDisplay(string aspectId, int aspectValue, ContentRepository instance)
    {
        AspectId = aspectId;
        Quantity = aspectValue;
        DisplayAspectImage(AspectId);
        DisplayQuantity(aspectValue);
        gameObject.name = "Aspect - " +aspectId;

    }

    public void ModifyQuantity(int quantityChange)
    {
       DisplayQuantity(Quantity+quantityChange);
    }
}
