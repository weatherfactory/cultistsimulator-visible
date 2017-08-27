using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Infrastructure;
using Noon;
using TMPro;
using UnityEditor;
using UnityEngine.EventSystems;

public interface ISituationOutputNote
{
    string TitleText { get; }
    string DescriptionText { get; }
}

public class SituationOutputNote : MonoBehaviour, ISituationOutputNote
{
    [SerializeField]private TextMeshProUGUI Title;
    [SerializeField]private TextMeshProUGUI Description;
    
    public string TitleText { get { return Title.text; } }
    public string DescriptionText { get { return Title.text; } }
    public void Initialise(INotification notification)
    {
        Title.text = notification.Title;
        Description.text = notification.Description;
    }


}


