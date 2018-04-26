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

        public List<Legacy> DetermineLegacies(Ending ending, List<IElementStack> stacksAtEnd)
        {
            //we always need three legacies.
            int randomLegaciesToDraw = 2; 

            List<Legacy> selectedLegacies=new List<Legacy>();

            //try to find a legacy that matches the death.
            Legacy deathDependentLegacy = _compendium.GetAllLegacies().Find(l => l.FromEnding == ending.Id);

            //there should be one! but in case there's not, log it, and get prepped to draw an extra random legacy.
            if (deathDependentLegacy == null)
            { 
                NoonUtility.Log("Couldn't find a death-dependent ending for " + ending.Id +
                                " so we'll draw a full 3 randoms. But this is a problem!");
                randomLegaciesToDraw++;
            }
            else
            { 
                selectedLegacies.Add(deathDependentLegacy);
            }

            //find: randomly selected legacies from the remaining list that are
            //- available for random draw
            //- not the death-dependent legacy, if there is one
            Random rnd=new Random();
            IEnumerable<Legacy> drawingLegacies = _compendium.GetAllLegacies().Where(l => l.AvailableWithoutEndingMatch)
                .OrderBy(l => rnd.Next());
            drawingLegacies = drawingLegacies.Take(randomLegaciesToDraw);
            drawingLegacies = drawingLegacies.Where(l=>l.Id!=(deathDependentLegacy==null ? "" : deathDependentLegacy.Id));

           selectedLegacies.AddRange(drawingLegacies);

            return selectedLegacies;
        }

        public LegacySelector(ICompendium compendium)
        {
            _compendium = compendium;
        }
    }
}