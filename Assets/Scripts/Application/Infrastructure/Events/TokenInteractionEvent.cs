using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SecretHistories.Constants.Events
{
    public enum Interaction { OnClicked, OnReceivedADrop,OnPointerEntered,OnPointerExited,OnDoubleClicked,OnDragBegin,OnDrag,OnDragEnd}

    public class TokenInteractionEventArgs
    {
        public Sphere Sphere { get; set; }
        public Element Element { get; set; }
        public Token Token { get; set; }
        public PointerEventData PointerEventData { get; set; }
        public Context Context { get; set; }
        public Interaction Interaction { get; set; }

        public TokenInteractionEventArgs()
        {
            Context = new Context(Context.ActionSource.Unknown);
        }

        public TokenInteractionEventArgs(Context context)
        {
            Context = context;
        }
    }
    
   

    [Serializable]
    public class TokenInteractionEvent : UnityEvent<TokenInteractionEventArgs>
    {
    }


}
