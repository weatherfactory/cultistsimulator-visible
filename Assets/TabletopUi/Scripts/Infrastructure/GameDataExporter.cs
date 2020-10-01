using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Interfaces;
using Noon;
using UnityEngine;
using UnityEngine.Analytics;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public interface IGameDataExporter
    {
        Hashtable GetSaveHashTable(MetaInfo metaInfo, ITableSaveState tableSaveState, Character character);

        Hashtable GetHashTableForStacks(IEnumerable<ElementStackToken> stacks);

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
        public Hashtable GetSaveHashTable(MetaInfo metaInfo, ITableSaveState saveState, Character character)
        {
            Hashtable htMeta;
            Hashtable htChar;
            Hashtable htDecks;
            Hashtable htStacks;
            Hashtable htSituations;

			htMeta = GetHashTableForMetaInfo(metaInfo);
            htChar = GetHashTableForCharacter(character);
            if (character.State==CharacterState.Viable)
                htDecks = GetHashTableForDecks(character.DeckInstances);
            else
            {
                htDecks = new Hashtable();
            }

            if(saveState.IsTableActive())
            {
             htStacks = GetHashTableForStacks(saveState.TableStacks);
			 htSituations = GetHashTableForSituations(saveState.Situations);
            }
            else
            {
                htStacks = new Hashtable();
                htSituations=new Hashtable();
            }


            // Build complete hashtable only if all subtables are valid
            var htAll = new Hashtable()
            {
                {SaveConstants.SAVE_METAINFO,			htMeta },
                {SaveConstants.SAVE_CHARACTER_DETAILS,	htChar },
                {SaveConstants.SAVE_ELEMENTSTACKS,		htStacks },
                {SaveConstants.SAVE_SITUATIONS,			htSituations },
                {SaveConstants.SAVE_DECKS,				htDecks }
            };
            return htAll;
        }

        private Hashtable GetHashTableForMetaInfo(MetaInfo metaInfo)
        {
            var htMetaInfo=new Hashtable();
            htMetaInfo.Add(SaveConstants.SAVE_VERSIONNUMBER,metaInfo.VersionNumber);
            return htMetaInfo;
        }

        private Hashtable GetHashTableForCharacter(Character character)
        {
            var htCharacter=new Hashtable();
			if (character == null)
			{
				string name = "none";
				htCharacter.Add(SaveConstants.SAVE_NAME,name);
			}
			else
			{
				htCharacter.Add(SaveConstants.SAVE_NAME,character.Name);
                if(character.EndingTriggered!=null)
                    htCharacter.Add(SaveConstants.SAVE_CURRENTENDING, character.EndingTriggered.Id);
                htCharacter.Add(SaveConstants.SAVE_PROFESSION, character.Profession);
                if(character.ActiveLegacy!=null)
                    htCharacter.Add(SaveConstants.SAVE_ACTIVELEGACY,character.ActiveLegacy.Id);

				var htExecutions=new Hashtable();
				foreach (var e in character.GetAllExecutions())
				{
					htExecutions.Add(e.Key,e.Value);
				}

				htCharacter.Add(SaveConstants.SAVE_EXECUTIONS, htExecutions);

				var htPastLevers = new Hashtable();
				foreach(var record in character.GetPreviousCharacterHistoryRecords())
					htPastLevers.Add(record.Key.ToString(), record.Value);

                htCharacter.Add(SaveConstants.SAVE_PAST_LEVERS,htPastLevers);


				var htFutureLevers = new Hashtable();
				foreach (var record in character.GetInProgressHistoryRecords())
					htFutureLevers.Add(record.Key.ToString(), record.Value);

				htCharacter.Add(SaveConstants.SAVE_FUTURE_LEVERS, htFutureLevers);
			}

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
				if (s.situationToken != null && s.situationToken.SaveLocationInfo != null)
				{
					var htSituationProperties = s.GetSaveData();
					htSituations.Add(s.situationToken.SaveLocationInfo, htSituationProperties);
				}
            }
            return htSituations;
        }
        /// <summary>
        /// return save data for all stacks. Each one in turn will be inspected.
        /// NOTE: stacks *in situations* are *not* handled by this function
        /// </summary>
        /// <returns></returns>
        public Hashtable GetHashTableForStacks(IEnumerable<ElementStackToken> stacks)
        {
            var htElementStacks = new Hashtable();
            foreach (var e in stacks)
            {
				if (e!=null && e.SaveLocationInfo!=null)
				{
					var stackHashtable=GetHashtableForThisStack(e);

					htElementStacks.Add(e.SaveLocationInfo, stackHashtable);
				}
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
            //TODO
            //var ht = new Hashtable();
            //var htMetaInfo = new Hashtable { { SaveConstants.SAVE_VERSIONNUMBER, _metaInfo.VersionNumber } };
            //ht.Add(SaveConstants.SAVE_METAINFO, htMetaInfo);
            ////TODO
            //AddMetaInfoToHashtable(ht); //in crosscenestate
            //TODO
            //AddCurrentEndingToHashtable(ht); //in crosscenestate
            //TODO
            //AddDefunctCharacterToHashtable(ht); //in crosscenestate

            return null;
        }

		private Hashtable GetHashtableForThisStack(ElementStackToken stack)
        {
            var htStackProperties = new Hashtable();
            htStackProperties.Add(SaveConstants.SAVE_ELEMENTID, stack.EntityId);
            htStackProperties.Add(SaveConstants.SAVE_QUANTITY, stack.Quantity);
			htStackProperties.Add(SaveConstants.LIFETIME_REMAINING, Mathf.CeilToInt(stack.LifetimeRemaining));
            htStackProperties.Add(SaveConstants.MARKED_FOR_CONSUMPTION,stack.MarkedForConsumption);
            htStackProperties.Add(SaveConstants.SAVE_LASTTABLEPOS_X, stack.LastTablePos.HasValue ? stack.LastTablePos.Value.x : 0.0f );
            htStackProperties.Add(SaveConstants.SAVE_LASTTABLEPOS_Y, stack.LastTablePos.HasValue ? stack.LastTablePos.Value.y : 0.0f );

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


		public void AnalyticsReport( bool success, MetaInfo metaInfo, Character character,IEnumerable<ElementStackToken> stacks, IEnumerable<SituationController> situationControllers,IEnumerable<IDeckInstance> deckInstances )
		{
			// Report very basic info	- success/failure (so we can measure failure %)
			//							- Data counts so we can see what is going missing (0 == empty, -1 == null ref)
			//							- one sample save location in case that is getting cleared
			int numStacks		= (stacks!=null) ?					stacks.Count() : -1;
			int numSituations	= (situationControllers!=null) ?	situationControllers.Count() : -1;
			int numDecks		= (deckInstances!=null) ?			deckInstances.Count() : -1;

			string stacksSaveLoc	= "n/a";
			if (numStacks>0)
			{
				stacksSaveLoc = stacks.ElementAt(0).SaveLocationInfo;
			}

			Analytics.CustomEvent( "autosave_report", new Dictionary<string,object>
			{
				{ "success",		success },
				{ "stacks",			numStacks },
				{ "stacks_save_loc",stacksSaveLoc },	// Sample save location so if a save fails with valid lists, we can see if the saveLocInfo was valid
				{ "situations",		numSituations },
				{ "decks",			numDecks }
			} );
		}
	}
}
