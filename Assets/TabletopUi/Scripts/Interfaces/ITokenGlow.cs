using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Interfaces;
using UnityEngine;

namespace Assets.CS.TabletopUI.Interfaces {
    public interface IGlowableView {
        void SetGlowColor(UIStyle.TokenGlowColor colorType);
        void SetGlowColor(Color color);
        void ShowGlow(bool glowState, bool instant = false);
    }
}
