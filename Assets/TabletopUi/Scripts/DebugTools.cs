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
    [SerializeField]private Button btnNextTrack;
    [SerializeField] private BackgroundMusic backgroundMusic;


    public void Awake()
    {
        btnPlusOne.onClick.AddListener(() => AddCard(adjustElementNamed.text));
        btnMinusOne.onClick.AddListener(() => DecrementElement(adjustElementNamed.text));
        btnFastForward.onClick.AddListener(() => FastForward(30));
        btnNextTrack.onClick.AddListener(NextTrack);
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

    void NextTrack()
    {
        backgroundMusic.PlayRandomClip();
    }
}

