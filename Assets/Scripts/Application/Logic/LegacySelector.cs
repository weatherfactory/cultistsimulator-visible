using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Fucine;


namespace Assets.Logic
{
    public class LegacySelector
    {
        private readonly Compendium _compendium;

        public List<Legacy> DetermineLegacies(Ending ending)
        {
            //we always need three legacies.
            int randomLegaciesToDraw = 2;
            Random rnd = new Random();

            List<Legacy> selectedLegacies=new List<Legacy>();


            //try to find a legacy that matches the death.
            Legacy endingDependentLegacy = _compendium.GetEntitiesAsList<Legacy>().OrderBy(l=>rnd.Next()).ToList().
                Find(l => l.FromEnding == ending.Id);

            if (endingDependentLegacy == null)
                endingDependentLegacy = NullLegacy.Create(); //because of the Linq above, can't rely on pulling a NullLegacy from the Compendium 

            //there should be one! but in case there's not, log it, and get prepped to draw an extra random legacy.
            if (!endingDependentLegacy.IsValid())
            {
                NoonUtility.Log("Couldn't find a death-dependent ending for " + ending.Id +
                                " so we'll draw a full 3 randoms. But this is a problem!",1);
                randomLegaciesToDraw++;
            }
            else
            {
                NoonUtility.Log($"Ending dependent legacy {endingDependentLegacy.Id} from ending {ending.Id}");
                selectedLegacies.Add(endingDependentLegacy);
            }

            //find: randomly selected legacies from the remaining list that are
            //- available for random draw
            //- not the death-dependent legacy, if there is one
            //- not excluded by the death-dependent legacy
            IEnumerable<Legacy> drawingLegacies = _compendium.GetEntitiesAsList<Legacy>().Where(
                    l => l.AvailableWithoutEndingMatch
                         && (!endingDependentLegacy.ExcludesOnEnding.Contains(l.Id)))
                .OrderBy(l => rnd.Next()).ToList();

            NoonUtility.Log(drawingLegacies.Count() + " legacies available to draw from");
            
            
            drawingLegacies = drawingLegacies.Where(l => l.Id != endingDependentLegacy.Id).Take(randomLegaciesToDraw);

            foreach(var d in drawingLegacies)
                NoonUtility.Log($"Drawn extra legacy: {d.Id}" );
            

            selectedLegacies.AddRange(drawingLegacies);



            return selectedLegacies;
        }

        public LegacySelector(Compendium compendium)
        {
            _compendium = compendium;
        }
    }
}
