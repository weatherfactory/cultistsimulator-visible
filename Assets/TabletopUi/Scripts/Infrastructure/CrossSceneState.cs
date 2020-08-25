using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Assets.Core;
using Assets.Core.Entities;
using Noon;
using OrbCreationExtensions;

namespace Assets.TabletopUi.Scripts.Infrastructure
{

   public static class __CrossSceneState
    {
  

        private static void AddDefunctCharacterToHashtable(Hashtable ht)
        {
            //  var c = GetDefunctCharacter();
            var c = new Character();
            if (c != null)
            {
                var htDC = new Hashtable();
                htDC.Add(SaveConstants.SAVE_NAME,c.Name);
                
                var htFutureLevers=new Hashtable();
                foreach (var record in c.GetInProgressHistoryRecords())
                    htFutureLevers.Add(record.Key.ToString(), record.Value);

                htDC.Add(SaveConstants.SAVE_FUTURE_LEVERS, htFutureLevers);

                ht.Add(SaveConstants.SAVE_DEFUNCT_CHARACTER_DETAILS, htDC);
            }            
        }






    }
}
