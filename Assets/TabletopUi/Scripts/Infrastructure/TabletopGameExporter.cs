using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Interfaces;
using Noon;
using OrbCreationExtensions;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public interface ITabletopGameExporter
    {
        Hashtable Export(IEnumerable<IElementStack> stacks, IEnumerable<ISituationAnchor> situations);
    }

    public class TabletopGameExporter : ITabletopGameExporter
    {
       private Hashtable ExportElementStacks(IEnumerable<IElementStack> stacks)
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

        private Hashtable ExportSituations(IEnumerable<ISituationAnchor> tokens)
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
           htAll.Add(Noon.NoonConstants.CONST_SAVE_ELEMENTSTACKS,ExportElementStacks(stacks));
            htAll.Add(Noon.NoonConstants.CONST_SAVE_SITUATIONS, ExportSituations(situations));
           return htAll;
       }
    }
}
