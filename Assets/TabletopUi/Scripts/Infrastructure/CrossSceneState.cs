using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Entities;
using OrbCreationExtensions;

namespace Assets.TabletopUi.Scripts.Infrastructure
{

    public class SavedCrossSceneState
    {
        public Ending CurrentEnding;
        public List<Legacy> AvailableLegacies=new List<Legacy>();


    }
    /// <summary>
    /// AFAICT there is no way to pass data between scenes on a load, except with a static object.
    /// </summary>
   public static class CrossSceneState
    {
        private static Ending _currentEnding;
        private static List<Legacy> _availableLegacies;
        private static Legacy _chosenLegacy;



        public static Hashtable GetHashTableForCrossSceneState()
        {
            var ht=new Hashtable();
            AddCurrentEndingToHashtable(ht);

            AddAvailableLegaciesToHashtable(ht);

            return ht;
        }

        private static void AddCurrentEndingToHashtable(Hashtable ht)
        {
            var currentEnding = GetCurrentEnding();
            if (currentEnding != null)
                ht.Add(SaveConstants.SAVE_CURRENTENDING, GetCurrentEnding().Id);
        }

        private static void AddAvailableLegaciesToHashtable(Hashtable ht)
        {
            var htLegacies = new Hashtable();
            var availableLegacies = GetAvailableLegacies();

            if (availableLegacies!=null)
            {
                foreach (var l in GetAvailableLegacies())
                {
                    htLegacies.Add(l.Id, l.Id);
                }
            }
            ht.Add(SaveConstants.SAVE_AVAILABLELEGACIES, htLegacies);
        }

        public static void SetCurrentEnding(Ending ending)
        {
            if(ending==null)
                throw new ApplicationException("Guard clause: use ClearEndingAndLegacies to set ending to null");
            _currentEnding = ending;
        }

        public static Ending GetCurrentEnding()
        {
            return _currentEnding;
        }

        public static void ClearEndingAndLegacies()
        {
            _currentEnding = null;
            _availableLegacies = null;
        }

        public static void SetChosenLegacy(Legacy chosen)
        {
            _chosenLegacy = chosen;
        }


        public static Legacy GetChosenLegacy()
        {
            return _chosenLegacy;
        }


        public static List<Legacy> GetAvailableLegacies()
        {

            return _availableLegacies;
        }

        public static void SetAvailableLegacies(List<Legacy> legacies)
        {
            _availableLegacies = legacies;
        }


    }
}
