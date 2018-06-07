using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.TabletopUi.Scripts.Interfaces;
using Noon;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public interface IGameDataExporter
    {
        Hashtable GetSaveHashTable(MetaInfo metaInfo, Character character, IEnumerable<IElementStack> stacks, IEnumerable<SituationController> situationControllers,IEnumerable<IDeckInstance> decks);

        Hashtable GetHashTableForStacks(IEnumerable<IElementStack> stacks);

        Hashtable GetHashTableForSituationNotes(IEnumerable<ISituationNote> notes);
        Hashtable GetHashtableForExtragameState(Legacy withActiveLegacy);
}

    public class GameDataExporter: IGameDataExporter
    {

        /// <summary>
        /// top-level function, subfunctions wrap everything else
        /// Currently we only have stacks and situations passes in (each of those is then investigated); more entities would need more params
        /// </summary>
        /// <returns>a hashtable ready to be jsonised (or otherwise stored)</returns>
        public Hashtable GetSaveHashTable(MetaInfo metaInfo, Character character,IEnumerable<IElementStack> stacks, IEnumerable<SituationController> situationControllers,IEnumerable<IDeckInstance> deckInstances)
        {
            var htAll = new Hashtable()
            {
                {SaveConstants.SAVE_METAINFO,GetHashTableForMetaInfo(metaInfo) },
                {SaveConstants.SAVE_CHARACTER_DETAILS,GetHashTableForCharacter(character) },
                {SaveConstants.SAVE_ELEMENTSTACKS, GetHashTableForStacks(stacks)},
                {SaveConstants.SAVE_SITUATIONS, GetHashTableForSituations(situationControllers)},
                {SaveConstants.SAVE_DECKS,GetHashTableForDecks(deckInstances) }
            };
            return htAll;
        }

        private object GetHashTableForMetaInfo(MetaInfo metaInfo)
        {
            var htMetaInfo=new Hashtable();
            htMetaInfo.Add(SaveConstants.SAVE_VERSIONNUMBER,metaInfo.VersionNumber);
            //Bird, Worm enigma persist
            if (PlayerPrefs.HasKey(NoonConstants.BIRDWORMSLIDER))
            { 
                htMetaInfo.Add(NoonConstants.BIRDWORMSLIDER, PlayerPrefs.GetFloat(NoonConstants.BIRDWORMSLIDER));
                htMetaInfo.Add("WeAwaitSTE", "Hello, Seeker. If you're here to decipher enigmas, familiarise yourself with the eidesis in which were presented the Lion, the Boar and the Bull, and present it in turn to the sky.");
            }
            return htMetaInfo;
        }

        private Hashtable GetHashTableForCharacter(Character character)
        {
            var htCharacter=new Hashtable();
            htCharacter.Add(SaveConstants.SAVE_NAME,character.Name);
            htCharacter.Add(SaveConstants.SAVE_PROFESSION, character.Profession);
            htCharacter.Add(SaveConstants.SAVE_ACTIVELEGACY,character.ActiveLegacy);

            var htExecutions=new Hashtable();
            foreach (var e in character.GetAllExecutions())
            {
                htExecutions.Add(e.Key,e.Value);
            }

            htCharacter.Add(SaveConstants.SAVE_EXECUTIONS, htExecutions);

            var htPastLevers = new Hashtable();
            foreach(var record in character.GetAllPastLegacyEventRecords())
                htPastLevers.Add(record.Key.ToString(), record.Value);

             htCharacter.Add(SaveConstants.SAVE_PAST_LEVERS,htPastLevers);


            var htFutureLevers = new Hashtable();
            foreach (var record in character.GetAllFutureLegacyEventRecords())
                htFutureLevers.Add(record.Key.ToString(), record.Value);

            htCharacter.Add(SaveConstants.SAVE_FUTURE_LEVERS, htFutureLevers);


            return htCharacter;
        }


        /// <summary>
        /// return save data for all ongoing situations. Each one in turn will be inspected.
        /// NOTE: stacks *in situations* are handled by this function
        /// At the moment, the functionality for this is in the tokens and controllers
        /// I think it probably shouldn't be
        /// </summary>
        /// <returns></returns>
        private Hashtable GetHashTableForSituations(IEnumerable<SituationController> situationControllers)
        {
   
            var htSituations = new Hashtable();
            foreach (var s in situationControllers)
            {
                var htSituationProperties = s.GetSaveData(); 
                htSituations.Add(s.situationToken.SaveLocationInfo, htSituationProperties);
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
               var stackHashtable=GetHashtableForThisStack(e);

                    htElementStacks.Add(e.SaveLocationInfo, stackHashtable);

            }
            return htElementStacks;
        }

        public Hashtable GetHashTableForDecks(IEnumerable<IDeckInstance> deckInstances)
        {
            var htDecks=new Hashtable();
            foreach (var d in deckInstances)
            {
                var deckHashTable = d.GetSaveData();
                htDecks.Add(d.Id,deckHashTable);
            }
            return htDecks;
        }

        public Hashtable GetHashtableForExtragameState(Legacy withActiveLegacy)
        {
            return CrossSceneState.GetSaveDataForCrossSceneState(withActiveLegacy);
        }

private Hashtable GetHashtableForThisStack(IElementStack stack)
        {
            var htStackProperties = new Hashtable();
            htStackProperties.Add(SaveConstants.SAVE_ELEMENTID, stack.EntityId);
            htStackProperties.Add(SaveConstants.SAVE_QUANTITY, stack.Quantity);
			htStackProperties.Add(SaveConstants.LIFETIME_REMAINING, Mathf.CeilToInt(stack.LifetimeRemaining));
            htStackProperties.Add(SaveConstants.MARKED_FOR_CONSUMPTION,stack.MarkedForConsumption);

            var currentMutations = stack.GetCurrentMutations();
            if (currentMutations.Any())
            {
                var htMutations = new Hashtable();
                foreach (var kvp in currentMutations)
                {
                    htMutations.Add(kvp.Key,kvp.Value);
                }
                htStackProperties.Add(SaveConstants.SAVE_MUTATIONS, htMutations);

            }

            var currentIlluminations = stack.GetCurrentIlluminations();
            if (currentIlluminations.Any())
            {
                var htIlluminations = new Hashtable();
                foreach (var kvp in currentIlluminations)
                {
                    htIlluminations.Add(kvp.Key, kvp.Value);
                }
                htStackProperties.Add(SaveConstants.SAVE_ILLUMINATIONS, htIlluminations);

            }

            return htStackProperties;
        }

        public Hashtable GetHashTableForSituationNotes(IEnumerable<ISituationNote> notes)
        {
            var htNotes=new Hashtable();
            foreach (var on in notes)
            {
                var htEachNote = new Hashtable();
                htEachNote.Add(SaveConstants.SAVE_TITLE, on.Description);

                htNotes.Add((htNotes.Keys.Count + 1).ToString(), htEachNote); //need that tostring! exporter doesn't cope well with int keys
            }

            return htNotes;
        }
        
    }
}
