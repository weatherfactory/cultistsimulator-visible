using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Entities;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    /// <summary>
    /// AFAICT there is no way to pass data between scenes on a load, except with a static object.
    /// </summary>
   public static class CrossSceneState
    {
        private static Ending _currentEnding;
        private static List<Legacy> _availableLegacies;
        private static Legacy _chosenLegacy;

        public static void SetCurrentEnding(Ending ending)
        {
            if(ending==null)
                throw new ApplicationException("Guard clause: use ClearEnding to set ending to null");
            _currentEnding = ending;
        }

        public static Ending GetCurrentEnding()
        {
            return _currentEnding;
        }

        public static void ClearEnding()
        {
            _currentEnding = null;
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
            if (_availableLegacies == null)
            {
                //default legacies so the screen can be tested in isolation
                var l1 = new Legacy("A", "Legacy A", "Legacy A lorem ipsum dolor...","heart");
                l1.Effects = new AspectsDictionary() { { "shilling", 2 } };

                var l2= new Legacy("B", "Legacy B", "Legacy B lorem ipsum dolor...", "moth");
                l1.Effects = new AspectsDictionary() { { "shilling", 2 } };

                var l3 = new Legacy("C", "Legacy C", "Legacy C lorem ipsum dolor...", "knock");
                l1.Effects = new AspectsDictionary() { { "shilling", 2 } };


                _availableLegacies = new List<Legacy> { l1, l2, l3 };
            }

            return _availableLegacies;
        }

        public static void SetAvailableLegacies(List<Legacy> legacies)
        {
            _availableLegacies = legacies;
        }


    }
}
