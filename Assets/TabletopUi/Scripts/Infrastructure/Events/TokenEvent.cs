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

    public class TokenEventArgs
    {
        public Sphere Container { get; set; }
        public Element Element { get; set; }
        public ElementStackToken Token { get; set; }
        public PointerEventData PointerEventData { get; set; }
        //room for eg a diff or the nature of the change
    }

    [Serializable]
    public class TokenEvent : UnityEvent<TokenEventArgs>
    {
    }


}
