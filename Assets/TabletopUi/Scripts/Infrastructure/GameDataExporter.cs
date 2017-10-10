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

        /// <summary>
        /// top-level function, subfunctions wrap everything else
        /// Currently we only have stacks and situations passes in (each of those is then investigated); more entities would need more params
        /// </summary>
        /// <returns>a hashtable ready to be jsonised (or otherwise stored)</returns>
        public Hashtable GetSaveHashTable(IEnumerable<IElementStack> stacks, IEnumerable<ISituationAnchor> situations)
        {
            var htAll = new Hashtable
           {
               {SaveConstants.SAVE_ELEMENTSTACKS, GetHashTableForStacks(stacks)},
               {SaveConstants.SAVE_SITUATIONS, GetHashTableForSituations(situations)}
           };
            return htAll;
        }
        /// <summary>
        /// return save data for all ongoing situations. Each one in turn will be inspected.
        /// NOTE: stacks *in situations* are handled by this function
        /// At the moment, the functionality for this is in the tokens and controllers
        /// I think it probably shouldn't be
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// return save data for all stacks. Each one in turn will be inspected.
        /// NOTE: stacks *in situations* are *not* handled by this function
        /// </summary>
        /// <returns></returns>
        public Hashtable GetHashTableForStacks(IEnumerable<IElementStack> stacks)
        {
            var htElementStacks = new Hashtable();
            foreach (var e in stacks)
            {
               var stackHashtable=GetHashtableForThisStack(e, htElementStacks);
                htElementStacks.Add(e.SaveLocationInfo, stackHashtable);
            }
            return htElementStacks;
        }

        private static Hashtable GetHashtableForThisStack(IElementStack e, Hashtable htElementStacks)
        {
            var htStackProperties = new Hashtable();
            htStackProperties.Add(SaveConstants.SAVE_ELEMENTID, e.Id);
            htStackProperties.Add(SaveConstants.SAVE_QUANTITY, e.Quantity);
            return htStackProperties;
        }

        public Hashtable GetHashtableForOutputNotes(IEnumerable<ISituationOutputNote> outputNotes)
        {
            var htOutputNotes=new Hashtable();
            foreach (var on in outputNotes)
            {
                var htEachOutputNotes = new Hashtable();
                htEachOutputNotes.Add(SaveConstants.SAVE_TITLE, on.TitleText);
                htEachOutputNotes.Add(SaveConstants.SAVE_DESCRIPTION, on.DescriptionText);

                htOutputNotes.Add((htOutputNotes.Keys.Count + 1).ToString(), htEachOutputNotes); //need that tostring! exporter doesn't cope well with int keys
            }

            return htOutputNotes;
        }
        
    }
}
