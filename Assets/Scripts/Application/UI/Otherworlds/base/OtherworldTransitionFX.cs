using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.UI.Otherworlds
{
    public abstract class OtherworldTransitionFX: MonoBehaviour
    {
        public abstract bool CanShow();
        public abstract bool CanHide();
        public abstract void Show(Action onShowComplete);

        public abstract void Hide(Action onHideComplete);

    }
}
