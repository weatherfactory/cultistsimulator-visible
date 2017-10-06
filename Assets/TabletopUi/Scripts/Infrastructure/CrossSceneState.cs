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

        public static void SetCurrentEnding(Ending ending)
        {
            currentEnding = ending;
        }

        public static Ending GetCurrentEnding()
        {
            return currentEnding;
        }

    }
}
