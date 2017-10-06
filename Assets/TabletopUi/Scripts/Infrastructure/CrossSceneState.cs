using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            return availableLegacies;
        }

        public static void SetAvailableLegacies(List<LegacyEntity> legacies)
        {
            availableLegacies = legacies;
        }

    }
}
