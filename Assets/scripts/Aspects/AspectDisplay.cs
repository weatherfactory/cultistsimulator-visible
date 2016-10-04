using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.UI;

public class AspectDisplay : MonoBehaviour
{
    public string AspectId;
    public int Quantity;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

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

    public void PopulateDisplay(string aspectId, int aspectValue, ContentManager instance)
    {
        AspectId = aspectId;
        Quantity = aspectValue;
        DisplayAspectImage(AspectId);
        DisplayQuantity(aspectValue);

    }

    public void ModifyQuantity(int quantity)
    {
        throw new System.NotImplementedException();
    }
}
