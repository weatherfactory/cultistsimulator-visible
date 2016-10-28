using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.UI;

public class VerbPanel : MonoBehaviour {

    [SerializeField]
    GameObject prefabVerbFrame;

    public void BlockVerb(string actionId)
    {
        DraggableVerbToken[] tokens = GetComponentsInChildren<DraggableVerbToken>();
        foreach (DraggableVerbToken dvt in tokens)
            if(dvt.Verb.Id==actionId)
                dvt.gameObject.SetActive(false);
    }

    public void UnblockVerb(string actionId)
    {
        DraggableVerbToken[] tokens = GetComponentsInChildren<DraggableVerbToken>();
        foreach (DraggableVerbToken dvt in tokens)
            if (dvt.Verb.Id == actionId)
                dvt.gameObject.SetActive(false);
    }

    public void AddVerbToPanel(Verb v)
    {
        GameObject verbFrame = Instantiate(prefabVerbFrame, transform) as GameObject;
        verbFrame.name = "Frame - " + v.Id;
        Image image = verbFrame.GetComponentsInChildren<Image>().Single(i => i.name == "VerbToken");
        Sprite sprite = ContentRepository.Instance.GetSpriteForVerb(v.Id);
        image.sprite = sprite;
        DraggableVerbToken token = verbFrame.GetComponentInChildren<DraggableVerbToken>();
        token.Verb = v;
    }
}
