using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using UnityEngine;

/// <summary>
/// just so I can mock it in tests
/// </summary>
   public class Dice: IDice
    {
    System.Random random = new System.Random();
        private IRollOverride _rollOverride;

    public int Rolld100(Recipe recipe = null)
    {
        var possibleOverride = _rollOverride.PopNextOverrideValue(recipe);
        if (possibleOverride > 0)
            return possibleOverride;
        else
            return random.Next(1,101);
        }

        public Dice(IRollOverride rollOverride)
        {
            _rollOverride = rollOverride;
        }

}



public interface IDice
{
    int Rolld100(Recipe recipe = null);
}
