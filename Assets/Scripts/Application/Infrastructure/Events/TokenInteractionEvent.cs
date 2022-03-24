using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SecretHistories.Constants.Events
{
 

    public class TokenInteractionEventArgs
    {
        public Sphere Sphere { get; private set; }
        public ITokenPayload Payload { get; private set; }
        public Token Token { get; private set; }
        public PointerEventData PointerEventData { get; private set; }
        public Context Context { get; private set; }
        public Interaction Interaction { get; private set; }

        public TokenInteractionEventArgs()
        {
            Context = new Context(Context.ActionSource.Unknown);
        }

        public TokenInteractionEventArgs(Context context)
        {
            Context = context;
        }

        public TokenInteractionEventArgs(PointerEventData pointerEventData, ITokenPayload payload, Token token, Sphere sphere, Interaction interaction) : this()
        {
            PointerEventData = pointerEventData;
            Payload = payload;
            Token = token;
            Sphere = sphere;
            Interaction = interaction;
        }
    }
    


}
