using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


    public interface IRecipeSituationSubscriber
    {
    /// <summary>
    /// SITUATIONBEGINS IS A HACK
    /// </summary>
        void SituationBegins(SituationInfo info,BaseRecipeSituation rs);
    void SituationUpdated(SituationInfo info);
    
    }

