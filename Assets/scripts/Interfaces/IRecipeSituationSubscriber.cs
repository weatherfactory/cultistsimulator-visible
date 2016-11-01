using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


    public interface IRecipeSituationSubscriber
    {
       void ReceiveSituationUpdate(Recipe recipe,RecipeTimerState state,float timeRemaining,SituationInfo info);
    }

