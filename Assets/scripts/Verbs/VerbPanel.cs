using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class VerbPanel : BoardMonoBehaviour {

    [SerializeField]
    GameObject prefabVerbFrame;
    Dictionary<string,DraggableVerbToken> blockedVerbs=new Dictionary<string, DraggableVerbToken>();

    public void BlockVerb(string actionId)
    {
        DraggableVerbToken[] tokens = GetComponentsInChildren<DraggableVerbToken>();
        foreach (DraggableVerbToken dvt in tokens)
            if (dvt.Verb.Id == actionId)
            {
                blockedVerbs.Add(actionId, dvt);
                dvt.gameObject.SetActive(false);
            }
                
    }

    public void UnblockVerb(string actionId)
    {
        if (blockedVerbs.ContainsKey(actionId))
        {
            blockedVerbs[actionId].gameObject.SetActive(true);
            blockedVerbs.Remove(actionId);
        }
    }

    public void AddVerbToPanel(Verb v)
    {
        GameObject verbFrame = Instantiate(prefabVerbFrame, transform) as GameObject;
        verbFrame.name = "Frame - " + v.Id;
        verbFrame.GetComponent<VerbFrame>().ForVerbId = v.Id;
        Image image = verbFrame.GetComponentsInChildren<Image>().Single(i => i.name == "VerbToken");
        Sprite sprite = ResourcesManager.GetSpriteForVerb(v.Id);
        image.sprite = sprite;
        DraggableVerbToken token = verbFrame.GetComponentInChildren<DraggableVerbToken>();
        token.Verb = v;
        token.HomeFrame = verbFrame;
    }
}
