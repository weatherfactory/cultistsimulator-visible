using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.CS.TabletopUI;

namespace Assets.TabletopUi.Scripts.Interfaces
{
    public interface IArtAnimatableToken:IToken
    {
        void StartArtAnimation();
        bool CanAnimateArt();
    }
}
