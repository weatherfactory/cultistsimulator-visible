using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using Assets.Core.Interfaces;
using Assets.TabletopUi.Scripts.Interfaces;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public interface IGameDataExporter
    {
        Hashtable GetSaveHashTable(IEnumerable<IElementStack> stacks, IEnumerable<ISituationAnchor> situations);
        Hashtable GetHashTableForStacks(IEnumerable<IElementStack> stacks);
    }

    public class GameDataExporter: IGameDataExporter
    {


        public Hashtable GetSaveHashTable(IEnumerable<IElementStack> stacks, IEnumerable<ISituationAnchor> situations)
        {
            var htAll = new Hashtable
           {
               {SaveConstants.SAVE_ELEMENTSTACKS, GetHashTableForStacks(stacks)},
               {SaveConstants.SAVE_SITUATIONS, GetHashTableForSituations(situations)}
           };
            return htAll;
        }

        public Hashtable GetHashTableForSituations(IEnumerable<ISituationAnchor> situations)
        {
   
            var htSituations = new Hashtable();
            foreach (var s in situations)
            {
                var htSituationProperties = s.GetSaveDataForSituation();
                htSituations.Add(s.SaveLocationInfo, htSituationProperties);
            }
            return htSituations;
        }

        public Hashtable GetHashTableForStacks(IEnumerable<IElementStack> stacks)
        {
            var htElementStacks = new Hashtable();
            foreach (var e in stacks)
            {
                var htStackProperties = new Hashtable();
                htStackProperties.Add(SaveConstants.SAVE_ELEMENTID, e.Id);
                htStackProperties.Add(SaveConstants.SAVE_QUANTITY, e.Quantity);
                htElementStacks.Add(e.SaveLocationInfo, htStackProperties);
            }
            return htElementStacks;
        }

        public Hashtable GetHashtableForOutputNotes(IEnumerable<ISituationOutputNote> outputNotes)
        {
            var htOutputNotes=new Hashtable();
            foreach (var on in outputNotes)
            {
                var htEachOutputNotes=new Hashtable();
                htEachOutputNotes.Add(SaveConstants.SAVE_TITLE, on.TitleText);
                htEachOutputNotes.Add(SaveConstants.SAVE_DESCRIPTION,on.DescriptionText);
                
                htOutputNotes.Add((htOutputNotes.Keys.Count+1).ToString() ,htEachOutputNotes); //need that tostring! exporter doesn't cope well with int keys
            }

            return htOutputNotes;
        }
        
    }
}
