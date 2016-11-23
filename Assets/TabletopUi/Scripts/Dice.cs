using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// just so I can mock it in tests
/// </summary>
   public class Dice: IDice
    {
    System.Random random = new System.Random();

    public int Rolld100()
        {
            
            return random.Next(1,101);
        }
    }

public interface IDice
{
    int Rolld100();
}
