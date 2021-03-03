using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Ghosts
{
    public interface IGhost
    {
        void FollowToken(Token forToken);
    }

    public class CardGhost: MonoBehaviour,IGhost
    {

        public void FollowToken(Token forToken)
        {
            transform.position = forToken.transform.position;
        }
    }
}
