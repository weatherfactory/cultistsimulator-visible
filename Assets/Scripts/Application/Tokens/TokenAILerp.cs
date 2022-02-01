using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pathfinding;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Tokens
{
    public class TokenAILerp: AILerp
    {
        public event Action<Token, Context> OnTokenArrival;

        public override void OnTargetReached()
        {
            var token = gameObject.GetComponent<Token>();
            if(OnTokenArrival!=null)
                OnTokenArrival.Invoke(token,Context.Unknown());
        }
    }
}
