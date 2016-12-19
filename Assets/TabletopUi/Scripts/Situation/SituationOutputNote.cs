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

public interface ISituationOutput
{
    string TitleText { get; }
    string DescriptionText { get; }
    ITokenTransformWrapper GetTokenTransformWrapper();
}

public class SituationOutputNote : MonoBehaviour, ISituationOutput
{
    [SerializeField]private TextMeshProUGUI Title;
    [SerializeField]private TextMeshProUGUI Description;
    [SerializeField]private SituationOutputTokenContainer cardTokenContainer;
    
    public string TitleText { get { return Title.text; } }
    public string DescriptionText { get { return Title.text; } }

    public void Initialise(INotification notification, IEnumerable<IElementStack> stacks,SituationOutputContainer soc)
    {
        Title.text = notification.Title;
        Description.text = notification.Description;
        cardTokenContainer.SetParentOutputContainer(soc);
        cardTokenContainer.GetElementStacksManager().AcceptStacks(stacks);
    }

    public ITokenTransformWrapper GetTokenTransformWrapper()
    {
       return new TokenTransformWrapper(cardTokenContainer.transform);
    }
}


