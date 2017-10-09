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
            _currentEnding = ending;
        }

        public static Ending GetCurrentEnding()
        {
            return _currentEnding;
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
                var l1 = new Legacy()
                {
                    Id = "A",
                    Label = "Legacy A",
                    Description = "Legacy A desc lorem ipsum dolor...",
                    ElementEffects = new AspectsDictionary() { { "shilling", 1 } },
                    Image="moth"
                };
                var l2 = new Legacy()
                {
                    Id = "B",
                    Label = "Legacy B",
                    Description = "Legacy B desc lorem ipsum dolor...",
                    ElementEffects = new AspectsDictionary() { { "shilling", 2 } },
                    Image = "heart"
                }; ;
                var l3 = new Legacy()
                {
                    Id = "C",
                    Label = "Legacy C",
                    Description = "Legacy C desc lorem ipsum dolor...",
                    ElementEffects = new AspectsDictionary() { { "shilling",3} ,{"fragmentglory",1}},
                    Image = "knock"
                };

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
