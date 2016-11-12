using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.CS.TabletopUI;


public interface ISituationWindowSubscriber
    {
        void SituationBegins(VerbBox box);
    void SituationUpdated(VerbBox box);
    
    }

