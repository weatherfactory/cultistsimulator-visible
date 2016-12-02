using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Elements;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;

public class TabletopContainer : MonoBehaviour,ITokenContainer
{


    public void TokenPickedUp(DraggableToken draggableToken)
    {

    }

    public IEnumerable<ISituationAnchor> GetAllSituationTokens()
    {
        return GetComponentsInChildren<ISituationAnchor>();
    }

    public void CloseAllSituationWindowsExcept(SituationToken except)
    {
        var situationTokens =GetTokenTransformWrapper().GetSituationTokens().Where(sw => sw != except);
        foreach (var situationToken in situationTokens)
        {
            if (DraggableToken.itemBeingDragged == null ||
                DraggableToken.itemBeingDragged.gameObject != situationToken.gameObject)

                situationToken.CloseSituation();
        }
    }

    public void CreateSituation(IVerb verb,string recipeid,string locatorId)
    {
        Registry.TabletopObjectBuilder.BuildNewTokenRunningRecipe(recipeid, locatorId, verb);
    }

    public void PutOnTable(DraggableToken token)
    {
        GetTokenTransformWrapper().Accept(token);

        token.RectTransform.anchoredPosition3D = new Vector3(token.RectTransform.anchoredPosition3D.x, token.RectTransform.anchoredPosition3D.y, 0f);
        token.RectTransform.localRotation = Quaternion.identity;
    }

    public bool AllowDrag { get { return true; } }



    public ElementStacksManager GetElementStacksManager()
    {
        return new ElementStacksManager(GetTokenTransformWrapper());
    }
    
    public ITokenTransformWrapper GetTokenTransformWrapper()
    {
       return new TabletopContainerTokenTransformWrapper(transform);
    }
}
