using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Tokens.TokenPayloads;
using UnityEngine;

namespace SecretHistories.UI
{
    public abstract class OtherworldTransitionFX: MonoBehaviour
    {
        public abstract bool CanShow();
        public abstract bool CanHide();
        public abstract void Show(Ingress activeIngress, Action onShowComplete);

        public abstract void Hide(Action onHideComplete);

        [SerializeField] protected string EntrySfxName;
        [SerializeField] protected string ExitSfxName;
        [SerializeField] protected string Music;

    }
}
