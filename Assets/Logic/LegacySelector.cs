using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Assets.Core;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Noon;

namespace Assets.Logic
{
    public class LegacySelector
    {
        private readonly ICompendium _compendium;

        public List<Legacy> DetermineLegacies(Ending ending)
        {
            //we always need three legacies.
            int randomLegaciesToDraw = 2;
            Random rnd = new Random();

            List<Legacy> selectedLegacies=new List<Legacy>();


            //try to find a legacy that matches the death.
            Legacy deathDependentLegacy = _compendium.GetEntitiesAsList<Legacy>().OrderBy(l=>rnd.Next()).ToList().Find(l => l.FromEnding == ending.Id);

            //there should be one! but in case there's not, log it, and get prepped to draw an extra random legacy.
            if (deathDependentLegacy == null)
            {
                NoonUtility.Log("Couldn't find a death-dependent ending for " + ending.Id +
                                " so we'll draw a full 3 randoms. But this is a problem!",1);
                randomLegaciesToDraw++;
            }
            else
            {
                selectedLegacies.Add(deathDependentLegacy);
            }

            //find: randomly selected legacies from the remaining list that are
            //- available for random draw
            //- not the death-dependent legacy, if there is one
            //- not excluded by the death-dependent legacy
            IEnumerable<Legacy> drawingLegacies = _compendium.GetEntitiesAsList<Legacy>().Where(
                    l => l.AvailableWithoutEndingMatch
                         && (deathDependentLegacy == null || !deathDependentLegacy.ExcludesOnEnding.Contains(l.Id)))
                .OrderBy(l => rnd.Next()).ToList();

            NoonUtility.Log(drawingLegacies.Count() + " legacies available to draw from");

            NoonUtility.Log(randomLegaciesToDraw + "  legacies drawing");


            if (deathDependentLegacy == null)
                drawingLegacies = drawingLegacies.Take(randomLegaciesToDraw);
            else
                drawingLegacies = drawingLegacies.Where(l => l.Id != deathDependentLegacy.Id).Take(randomLegaciesToDraw);

           selectedLegacies.AddRange(drawingLegacies);



            return selectedLegacies;
        }

        public LegacySelector(ICompendium compendium)
        {
            _compendium = compendium;
        }
    }
}
