using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.CS.TabletopUI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Assets.TabletopUi.Scripts.Infrastructure.Events
{

    public enum TokenInteractionType{BeginDrag=1,EndDrag=2}
    public class TokenInteractionEventArgs
    {
        public IToken Token { get; set; }
        public TokenInteractionType TokenInteractionType { get; set; }
        public PointerEventData PointerEventData { get; set; }

    }

    [Serializable]
    public class TokenInteractionEvent:UnityEvent<TokenInteractionEventArgs>
    {
    }
}
