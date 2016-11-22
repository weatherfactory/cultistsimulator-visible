using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.CS.TabletopUI;

namespace Assets.TabletopUi
{
   public  class SituationController
   {
       public readonly SituationToken situationToken;
       private SituationWindow _window;
       private readonly SlotsContainer _startingSlots;
       private readonly SlotsContainer _ongoingSlots;
    public SituationWindow linkedWindow { get { return _window;} }


       public SituationController(SituationToken t, SituationWindow w,SlotsContainer startingSlots, SlotsContainer ongoingSlots)
       {
           situationToken = t;
           _window = w;
           _startingSlots = startingSlots;
           _ongoingSlots = ongoingSlots;
       }

       public void DisplayCurrentRecipe()
       {
            AspectsDictionary allAspects=new AspectsDictionary();
            
       }

       public void DisplayRecipeForAspects(AspectsDictionary aspects)
       {
           _window.DisplayRecipeForAspects(aspects);
       }

       public void PopulateAndShowWindow()
       {
            linkedWindow.PopulateAndShow(this);
        }
   }
}
