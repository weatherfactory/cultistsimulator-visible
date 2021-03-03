using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SecretHistories.UI
{
 public class TokenGhostDisplay: MonoBehaviour
    {

        public bool DisplayGhost(Token forToken)
        {
            Debug.Log($"{forToken.name} over {gameObject.name}");
            return true;
        }
    }
}
