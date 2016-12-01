using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Interfaces;
using OrbCreationExtensions;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
   public class TabletopGameExporter
   {
       public Hashtable ExportElementStacks(IEnumerable<IElementStack> stacks)
       {
           var htElementStacks=new Hashtable();
           foreach (var e in stacks)
           {
                var htStackProperties=new Hashtable();
               htStackProperties.Add(e.Id, e.Quantity);
                htElementStacks.Add(e.LocatorId,htStackProperties);   
           }
           return htElementStacks;
       }

        public Hashtable ExportSituations(IEnumerable<ISituationAnchor> tokens)
        {
            var htSituationTokens = new Hashtable();
            foreach (var s in tokens)
            {
                var htStackProperties = new Hashtable();
                htStackProperties.Add(s.Id, "--");
                htSituationTokens.Add(s.LocatorId, htStackProperties);
            }
            return htSituationTokens;
        }

       public Hashtable Export(IEnumerable<IElementStack> stacks, IEnumerable<ISituationAnchor> situations)
       {
           var htAll=new Hashtable();
           htAll.Add("elementStacks",ExportElementStacks(stacks));
            htAll.Add("situations", ExportSituations(situations));
           return htAll;
       }
    }
}
