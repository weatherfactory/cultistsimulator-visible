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
    [SerializeField] private Button btnPlusOne;
    [SerializeField] private Button btnMinusOne;


    public void Awake()
    {
        btnPlusOne.onClick.AddListener(delegate {AddCard(adjustElementNamed.text);});
        btnMinusOne.onClick.AddListener(delegate {DecrementElement(adjustElementNamed.text);});
    }
    void AddCard(string elementId)
    {
       
        if (Registry.Compendium.GetElementById(elementId) == null)
            Debug.Log("Can't find element with id " + elementId);
        else
        {
            IElementStacksWrapper wrapper = new TabletopElementStacksWrapper(cardHolder);
            ElementStacksGateway esg = new ElementStacksGateway(wrapper);
            esg.IncreaseElement(elementId, 1);
        }
    }

    void DecrementElement(string elementId)
    {
        if (Registry.Compendium.GetElementById(elementId) == null)
            Debug.Log("Can't find element with id " + elementId);
        else
        {
            IElementStacksWrapper wrapper = new TabletopElementStacksWrapper(cardHolder);
            ElementStacksGateway esg = new ElementStacksGateway(wrapper);
            esg.ReduceElement(elementId, -1);
        }
    }
}

