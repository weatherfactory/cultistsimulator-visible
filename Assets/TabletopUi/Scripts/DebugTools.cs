using UnityEngine;
using System.Collections;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts;
using UnityEngine.UI;

public class DebugTools : MonoBehaviour
{
    [SerializeField] private TabletopContainer tabletopContainer;
    [SerializeField] private Heart heart;
    [SerializeField] private InputField adjustElementNamed;
    [SerializeField] private Button btnPlusOne;
    [SerializeField] private Button btnMinusOne;
    [SerializeField] private Button btnFastForward;


    public void Awake()
    {
        btnPlusOne.onClick.AddListener(delegate {AddCard(adjustElementNamed.text);});
        btnMinusOne.onClick.AddListener(delegate {DecrementElement(adjustElementNamed.text);});
        btnFastForward.onClick.AddListener(delegate { FastForward(30); });

    }
    void AddCard(string elementId)
    {
        tabletopContainer.GetElementStacksManager().ModifyElementQuantity(elementId,1);
    }

    void DecrementElement(string elementId)
    {
        tabletopContainer.GetElementStacksManager().ModifyElementQuantity(elementId, -1);
    }

    void FastForward(float interval)
    {
            heart.Beat(interval);
    }
}

