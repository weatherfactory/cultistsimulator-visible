using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CurrentAspects : BoardMonoBehaviour
{

    [SerializeField] private GameObject objLimbo;
    [SerializeField]private GameObject prefabAspectFrame;

    private AspectFrame GetAspectFrameForId(string aspectId)
    {
        return
            GetComponentsInChildren<AspectFrame>().SingleOrDefault(a => a.AspectId == aspectId);
    }

    private void AddAspectToDisplay(string aspectId, int quantity)
    {
        GameObject newAspectDisplay = Instantiate(prefabAspectFrame, transform) as GameObject;
        if (newAspectDisplay != null)
        {
            AspectFrame aspectFrame = newAspectDisplay.GetComponent<AspectFrame>();
            aspectFrame.PopulateDisplay(aspectId, quantity, ContentManager.Instance);
        }
    }

    private void ChangeAspectQuantityInFrame(string aspectId, int quantity)
    {
        AspectFrame existingAspect = GetAspectFrameForId(aspectId);
        if (existingAspect)
            existingAspect.ModifyQuantity(quantity);
        else
            AddAspectToDisplay(aspectId, quantity);
    }



    public void ResetAspects()
    {
        //this drove me fucking nuts. It's my old nemesis: removing items from a collection as you go
        //but Unity lets you do it with transform
        //but the SetParent actually alters the collection
        //so foreach and standard iteration don't work. Counting down to 0 works.
        //and of course the SetParent to limbo is because Destroy happens at frame end.
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
            transform.GetChild(i).SetParent(objLimbo.transform);

        }
    }

    public void UpdateAspects(GameObject pnlWorkspace)
    {
      

        DraggableElementToken[] elements = pnlWorkspace.GetComponentsInChildren<DraggableElementToken>();

        foreach (DraggableElementToken draggableElementDisplay in elements)
        {

            foreach (KeyValuePair<string, int> kvp in draggableElementDisplay.Element.Aspects)
            {
                ChangeAspectQuantityInFrame(kvp.Key, kvp.Value);
            }

        }



    }
}
