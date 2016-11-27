using UnityEngine;
using System.Collections;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Elements;
using Assets.TabletopUi.Scripts.Services;

public class TabletopContainer : MonoBehaviour,ITokenContainer
{

    [SerializeField] private TabletopManager tabletopManager;

    public void TokenPickedUp(DraggableToken draggableToken)
    {

    }

    public void CloseAllSituationWindowsExcept(SituationToken situationToken)
    {
       tabletopManager.CloseAllSituationWindowsExcept(situationToken);
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
       return new TabletopTokenTransformWrapper(transform);
    }
}
