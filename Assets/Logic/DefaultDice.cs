using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;

namespace Assets.Core
{
    /// <summary>
    /// the DefaultDice always rolls 100; in other words, it will always fail non-100% recipe chances
    /// we use it for recipe predictions, so that the player never sees recipe predictions which aren't definite
    /// </summary>
   public class DefaultDice: IDice
    {
        public int Rolld100(Recipe recipe = null)
        {
            return 100;
        }
    }
}
