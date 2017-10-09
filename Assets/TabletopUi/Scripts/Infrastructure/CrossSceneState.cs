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
        private static Ending currentEnding;
        private static List<LegacyEntity> availableLegacies;

        public static void SetCurrentEnding(Ending ending)
        {
            currentEnding = ending;
        }

        public static Ending GetCurrentEnding()
        {
            return currentEnding;
        }

        public static List<LegacyEntity> GetAvailableLegacies()
        {
            if (availableLegacies == null)
            {
                //default legacies so the screen can be tested in isolation
                var l1 = new LegacyEntity()
                {
                    Id = "A",
                    Label = "Legacy A",
                    Description = "Legacy A desc lorem ipsum dolor...",
                    ElementEffects = new AspectsDictionary() { { "shilling", 1 } }
                };
                var l2 = new LegacyEntity()
                {
                    Id = "B",
                    Label = "Legacy B",
                    Description = "Legacy B desc lorem ipsum dolor...",
                    ElementEffects = new AspectsDictionary() { { "shilling", 2 } }
                }; ;
                var l3 = new LegacyEntity()
                {
                    Id = "C",
                    Label = "Legacy C",
                    Description = "Legacy C desc lorem ipsum dolor...",
                    ElementEffects = new AspectsDictionary() { { "shilling",3} }
                };

                availableLegacies = new List<LegacyEntity> { l1, l2, l3 };
            }

            return availableLegacies;
        }

        public static void SetAvailableLegacies(List<LegacyEntity> legacies)
        {
            availableLegacies = legacies;
        }


    }
}
