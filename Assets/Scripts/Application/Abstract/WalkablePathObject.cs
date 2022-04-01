using System.Collections;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Abstract
{
    public abstract class WalkablePathObject: MonoBehaviour
    {
        public abstract bool TokenAllowedHere(Token token);

    }
}