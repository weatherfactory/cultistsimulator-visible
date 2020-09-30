using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using UnityEngine.EventSystems;

namespace Assets.TabletopUi.Scripts.Interfaces
{
    public interface ITokenObserver
    {
        void OnStackClicked(ElementStackToken stack, PointerEventData pointerEventData, Element element);
        void OnStackDropped(ElementStackToken stack, EventArgs eventData);

        void OnStackPointerEntered(ElementStackToken stack, PointerEventData pointerEventData);
        void OnStackPointerExited(ElementStackToken stack, PointerEventData pointerEventData);


        void OnStackDoubleClicked(ElementStackToken elementStackToken, PointerEventData eventData, Element element);
    }
}
