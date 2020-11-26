using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Assets.TabletopUi.Scripts.Infrastructure.Events
{
    public enum Interaction { OnClicked, OnReceivedADrop,OnPointerEntered,OnPointerExited,OnDoubleClicked,OnDragBegin,OnDrag,OnDragEnd}

    public class TokenInteractionEventArgs
    {
        public Sphere Sphere { get; set; }
        public Element Element { get; set; }
        public Token Token { get; set; }
        public PointerEventData PointerEventData { get; set; }
        public Interaction Interaction { get; set; }

        //room for eg a diff or the nature of the change
    }

    [Serializable]
    public class TokenInteractionEvent : UnityEvent<TokenInteractionEventArgs>
    {
    }


}
