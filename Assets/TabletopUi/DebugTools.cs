using UnityEngine;
using System.Collections;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts;
using UnityEngine.UI;

public class DebugTools : MonoBehaviour
{
    [SerializeField] private Transform cardHolder;
    [SerializeField] private InputField adjustElementNamed;

    void AddCard()
    {
        string elementId = "";
        if (Registry.Compendium.GetElementById(elementId) == null)
            Debug.Log("Can't find element with id " + elementId);
        else
        {
            IElementStacksWrapper wrapper = new TabletopElementStacksWrapper(cardHolder);
            ElementStacksGateway esg = new ElementStacksGateway(wrapper);
            esg.IncreaseElement(elementId, 1);
        }
    }
}

