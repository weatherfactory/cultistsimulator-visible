using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Interfaces;
using Noon;
using OrbCreationExtensions;
using UnityEngine;
using UnityEngine.Assertions;
using Assets.TabletopUi.Scripts.Services;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public interface IGameDataImporter
    {
        void ImportSavedGameToState(TabletopTokenContainer tabletop, IGameEntityStorage storage, Hashtable htSave);
        SavedCrossSceneState ImportCrossSceneState(Hashtable htSave);
    }

    public class GameDataImporter : IGameDataImporter
    {
        private ICompendium compendium;

        
        public GameDataImporter(ICompendium compendium)
        {
            this.compendium = compendium;
        }



        public void ImportSavedGameToState(TabletopTokenContainer tabletop,IGameEntityStorage storage, Hashtable htSave)
        {
            var htCharacter = htSave.GetHashtable(SaveConstants.SAVE_CHARACTER_DETAILS);
            var htElementStacks = htSave.GetHashtable(SaveConstants.SAVE_ELEMENTSTACKS);
            var htSituations = htSave.GetHashtable(SaveConstants.SAVE_SITUATIONS);
            var htDecks = htSave.GetHashtable(SaveConstants.SAVE_DECKS);

            ImportCharacter(storage, htCharacter);
            //update the compendium text with tokens for this character
            compendium.SupplyLevers(storage);

            ImportTabletopElementStacks(tabletop, htElementStacks);

            ImportSituations(tabletop, htSituations);

            ImportDecks(storage, htDecks);

        }

        private void ImportCharacter(IGameEntityStorage storage, Hashtable htCharacter)
        {
            if(htCharacter.ContainsKey(SaveConstants.SAVE_NAME))
                storage.Name = htCharacter[SaveConstants.SAVE_NAME].ToString();


            if (htCharacter.ContainsKey(SaveConstants.SAVE_PROFESSION))
                storage.Profession = htCharacter[SaveConstants.SAVE_PROFESSION].ToString();


            var chosenLegacyForCharacterId =TryGetStringFromHashtable(htCharacter, SaveConstants.SAVE_ACTIVELEGACY);
            Legacy chosenLegacyForCharacter;
            if (string.IsNullOrEmpty(chosenLegacyForCharacterId))
                chosenLegacyForCharacter =
                    compendium.GetAllLegacies()
                        .First(); //support active legacies for characters who preceded saved active legacies
            else
                chosenLegacyForCharacter = compendium.GetLegacyById(chosenLegacyForCharacterId);

            storage.ActiveLegacy = chosenLegacyForCharacter;


            if (htCharacter.ContainsKey(SaveConstants.SAVE_EXECUTIONS))
            {
                var htExecutions = htCharacter.GetHashtable(SaveConstants.SAVE_EXECUTIONS);
                foreach(var key in htExecutions.Keys)
                    storage.AddExecutionsToHistory(key.ToString(),GetIntFromHashtable(htExecutions,key.ToString()));
            }

            if (htCharacter.ContainsKey(SaveConstants.SAVE_PAST_LEVERS))
            {
                var htPastLevers = htCharacter.GetHashtable(SaveConstants.SAVE_PAST_LEVERS);
                foreach (var key in htPastLevers.Keys)
                {
                    var enumKey = (LegacyEventRecordId) Enum.Parse(typeof(LegacyEventRecordId), key.ToString());
                    string value = htPastLevers[key].ToString();
                    if(!string.IsNullOrEmpty(value))
                        storage.SetOrOverwritePastLegacyEventRecord(enumKey, htPastLevers[key].ToString());

                }
            }

            if (htCharacter.ContainsKey(SaveConstants.SAVE_FUTURE_LEVERS))
            {
                var htFutureLevers = htCharacter.GetHashtable(SaveConstants.SAVE_FUTURE_LEVERS);
                foreach (var key in htFutureLevers.Keys)
                {
                    var enumKey = (LegacyEventRecordId)Enum.Parse(typeof(LegacyEventRecordId), key.ToString());
                    storage.SetFutureLegacyEventRecord(enumKey, htFutureLevers[key].ToString());

                }
            }
        }


        public SavedCrossSceneState ImportCrossSceneState(Hashtable htSave)
        {
            SavedCrossSceneState crossSceneState=new SavedCrossSceneState();
            
            //load current ending, if it exists
            if (htSave.ContainsKey(SaveConstants.SAVE_CURRENTENDING))
              crossSceneState.CurrentEnding =compendium.GetEndingById(htSave[SaveConstants.SAVE_CURRENTENDING].ToString());

            //load legacies, if they exist
            var htLegacies = htSave.GetHashtable(SaveConstants.SAVE_AVAILABLELEGACIES);
            
            foreach (var k in htLegacies.Keys)
            {
                Legacy l = compendium.GetLegacyById(k.ToString());
                if(l!=null)
                    crossSceneState.AvailableLegacies.Add(l);
            }

            //load previous character state, if it exists
            var htDefunctCharacter = htSave.GetHashtable(SaveConstants.SAVE_DEFUNCT_CHARACTER_DETAILS);
            if (htDefunctCharacter != null)
            {
                var chosenLegacyForDefunctCharacterId = TryGetStringFromHashtable(htDefunctCharacter, SaveConstants.SAVE_ACTIVELEGACY);
                Legacy chosenLegacyForDefunctCharacter;
                if (string.IsNullOrEmpty(chosenLegacyForDefunctCharacterId))
                    chosenLegacyForDefunctCharacter =
                        compendium.GetAllLegacies()
                            .First(); //support active legacies for characters who preceded saved active legacies
                else
                    chosenLegacyForDefunctCharacter = compendium.GetLegacyById(chosenLegacyForDefunctCharacterId);

                Character defunctCharacter=new Character(chosenLegacyForDefunctCharacter);

                if (htDefunctCharacter.ContainsKey(SaveConstants.SAVE_FUTURE_LEVERS))
                {
                    var htFutureLevers = htDefunctCharacter.GetHashtable(SaveConstants.SAVE_FUTURE_LEVERS);
                    foreach (var key in htFutureLevers.Keys)
                    {
                        var enumKey = (LegacyEventRecordId)Enum.Parse(typeof(LegacyEventRecordId), key.ToString());
                        defunctCharacter.SetFutureLegacyEventRecord(enumKey, htFutureLevers[key].ToString());

                    }
                }


                crossSceneState.DefunctCharacter = defunctCharacter;

                

            }


            return crossSceneState;
        }

        private void ImportTabletopElementStacks(TabletopTokenContainer tabletop, Hashtable htElementStacks)
        {

            var elementStackSpecifications = PopulateElementStackSpecificationsList(htElementStacks);

			//
			// BEGIN SAVE REPAIR
			//
			// Failsafe save repair - we know that the save can sometimes fill with duplicate cards causing massive slowdown. - CP
			// This looks for identical dupes and removes them, so the savegame is useful again.
			float startTime = Time.timeSinceLevelLoad;
			int dupeCount = 0;
			bool[] dupes = new bool[ elementStackSpecifications.Count() ];	// Alloc and init a bool per card in the list. Allows quick discards.
			for ( int n=0; n<dupes.Length; n++)
				dupes[n] = false;
			for ( int i=0; i<elementStackSpecifications.Count()-1; i++ )	// Iterate from 0 to all but the last entry
			{
				var stackToTest = elementStackSpecifications.ElementAt(i);

				// Expecting a LocationInfo of the form "100_0_hashcode" or "Work_hashcode"
				int underscore1 = stackToTest.LocationInfo.IndexOf( '_' );					// Find first underscore
				int underscore2 = stackToTest.LocationInfo.IndexOf( '_', underscore1+1 );	// Find second (optional) underscore
				string locString = stackToTest.LocationInfo.Truncate( underscore2>0?underscore2:underscore1 );	// Trim so we can compare the non-hashcode bit

				// Compare current card against all cards after it in the list
				for ( int j=i+1; j<elementStackSpecifications.Count(); j++ )	
				{
					var stackToCompare = elementStackSpecifications.ElementAt(j);
					
					if (dupes[j])	
						continue;	// Already marked this entry as a dupe - next!

					if (stackToTest.ElementId.Equals( stackToCompare.ElementId ) == false)
						continue;	// Different card type - next!

					string tempLoc = stackToCompare.LocationInfo.Truncate( locString.Length );
					if (tempLoc.Equals( locString ) == false)
						continue;	// Different position - next!

					dupes[j] = true;
					dupeCount++;
				}
			}

			for ( int n=dupes.Length-1; n>=0; n--)
			{
				if (dupes[n])
					elementStackSpecifications.RemoveAt( n );
			}
			float repairTime = Time.timeSinceLevelLoad - startTime;
			Debug.Log("Repaired " + dupeCount + " duplicates in " + repairTime + "s");
			//
			// END SAVE REPAIR
			//

            foreach (var ess in elementStackSpecifications)
            {
                tabletop.GetElementStacksManager().AcceptStack(tabletop.ReprovisionExistingElementStack(ess,Source.Existing(),ess.LocationInfo),new Context(Context.ActionSource.Loading));
            }

            //This code should now be obsolete! everything goes through reprovision. Leaving for the mo in case I missed something
            //foreach (var locationInfo in htElementStacks.Keys)
            //{
            //    var dictionaryElementStacks =
            //        NoonUtility.HashtableToStringStringDictionary(htElementStacks.GetHashtable(locationInfo));

            //    int quantity;
            //    var couldParse = Int32.TryParse(dictionaryElementStacks[SaveConstants.SAVE_QUANTITY], out quantity);
            //    if (!couldParse)
            //        throw new ArgumentException("Couldn't parse " + dictionaryElementStacks[SaveConstants.SAVE_QUANTITY] + " for " +
            //                                    dictionaryElementStacks[SaveConstants.SAVE_ELEMENTID] + " as a valid quantity.");

            //    tabletop.GetElementStacksManager().IncreaseElement(dictionaryElementStacks[SaveConstants.SAVE_ELEMENTID], 
            //        quantity,Source.Existing(), new Context(Context.ActionSource.Loading), locationInfo.ToString());
            //}
        }

        private void ImportDecks(IGameEntityStorage storage, Hashtable htDeckInstances)
        {
            foreach (var k in htDeckInstances.Keys)
            {
                var htEachDeck = htDeckInstances.GetHashtable(k);

                IDeckSpec spec = compendium.GetDeckSpecById(k.ToString());

                if (spec == null)
                    NoonUtility.Log("no deckspec found for saved deckinstance " + k.ToString());
                else
                { 
                    IDeckInstance deckInstance =  new DeckInstance(spec);
                    if (htEachDeck.ContainsKey(SaveConstants.SAVE_ELIMINATEDCARDS))
                    { 
                        ArrayList alEliminated = htEachDeck.GetArrayList(SaveConstants.SAVE_ELIMINATEDCARDS);
                        htEachDeck.Remove(SaveConstants.SAVE_ELIMINATEDCARDS);

                       foreach(var e in alEliminated)
                           deckInstance.AddToEliminatedCards(e.ToString() );
                    }


                    //Now we assume that the remaining keys are contiguous integers starting at 1
                    for (int i=1;i<=htEachDeck.Count;i++)
                    deckInstance.Add(htEachDeck[i.ToString()].ToString());

                storage.DeckInstances.Add(deckInstance);
                }
            }

            //That's all the saved decks;          
            //But it's possible a deck spec has been added since the game was saved, in a new version of the game.
            //Create and reset any new deckspecs

            foreach (var ds in compendium.GetAllDeckSpecs())
            {
                if (storage.GetDeckInstanceById(ds.Id) == null)
                {
                    IDeckInstance di = new DeckInstance(ds);
                    storage.DeckInstances.Add(di);
                    di.Reset();
                }
            }
        }

        private void ImportSituations(TabletopTokenContainer tabletop, Hashtable htSituations)
        {
            foreach (var locationInfo in htSituations.Keys)
            {
                var htSituationValues =htSituations.GetHashtable(locationInfo);

                string recipeId = TryGetStringFromHashtable(htSituationValues, SaveConstants.SAVE_RECIPEID);
                var recipe = compendium.GetRecipeById(recipeId);

                string verbId= htSituationValues[SaveConstants.SAVE_VERBID].ToString();
                
                IVerb situationVerb = compendium.GetVerbById(verbId);

                //This caters for the otherwise troublesome situation where a completed situation (no recipe) has been based on a created verb (no verb obj).
                if (situationVerb == null && recipe==null)
                    situationVerb = new CreatedVerb(verbId, "","");

                var command = new SituationCreationCommand(situationVerb, recipe, (SituationState)Enum.Parse(typeof(SituationState), htSituationValues[SaveConstants.SAVE_SITUATIONSTATE].ToString()));
                command.TimeRemaining = TryGetNullableFloatFromHashtable(htSituationValues, SaveConstants.SAVE_TIMEREMAINING);

                command.OverrideTitle = TryGetStringFromHashtable(htSituationValues, SaveConstants.SAVE_TITLE);
                command.CompletionCount = GetIntFromHashtable(htSituationValues, SaveConstants.SAVE_COMPLETIONCOUNT);

                var situationAnchor = CreateSituation(command, locationInfo.ToString());
                var situationController = situationAnchor.SituationController;

				// Import window state so we can restore the desktop roughly how the player left it - CP
				situationController.IsOpen = htSituationValues[SaveConstants.SAVE_SITUATION_WINDOW_OPEN].MakeBool();
				Vector3 pos = situationController.situationWindow.Position;
				pos.x = htSituationValues[SaveConstants.SAVE_SITUATION_WINDOW_X].MakeFloat();
				pos.y = htSituationValues[SaveConstants.SAVE_SITUATION_WINDOW_Y].MakeFloat();
				pos.z = htSituationValues[SaveConstants.SAVE_SITUATION_WINDOW_Z].MakeFloat();
				situationController.RestoreWindowPosition = pos;

                ImportSituationNotes(htSituationValues, situationController);

                ImportSlotContents(htSituationValues, situationController, tabletop, SaveConstants.SAVE_STARTINGSLOTELEMENTS);
                ImportSlotContents(htSituationValues, situationController, tabletop, SaveConstants.SAVE_ONGOINGSLOTELEMENTS);
                

                ImportSituationStoredElements(htSituationValues, situationController);
                ImportOutputs(htSituationValues, situationController, tabletop);

            }
        }

        ISituationAnchor CreateSituation(SituationCreationCommand creationCommand, string locatorInfo = null) {
            return Registry.Retrieve<SituationBuilder>().CreateTokenWithAttachedControllerAndSituation(creationCommand, locatorInfo);
        }

        private void ImportOutputs(Hashtable htSituationValues, SituationController situationController, TabletopTokenContainer tabletop)
        {
         var outputStacks=ImportOutputStacks(htSituationValues, tabletop);
            situationController.SetOutput(outputStacks);

        }

        private List<IElementStack> ImportOutputStacks(Hashtable htSituationValues, TabletopTokenContainer tabletop)
        {
            List<IElementStack> outputStacks = new List<IElementStack>();

            if (htSituationValues.ContainsKey(SaveConstants.SAVE_SITUATIONOUTPUTSTACKS))
            {

                var htSituationOutputStacks = htSituationValues.GetHashtable(SaveConstants.SAVE_SITUATIONOUTPUTSTACKS);

                    
                    var stackSpecification = PopulateElementStackSpecificationsList(htSituationOutputStacks);
                    foreach (var ess in stackSpecification)
                    {
                        outputStacks.Add(tabletop.ReprovisionExistingElementStack(ess,Source.Existing()));
                    }

            }
            return outputStacks;
        }

        private void ImportSituationNotes(Hashtable htSituationValues,SituationController controller)
        {
            if (htSituationValues.ContainsKey(SaveConstants.SAVE_SITUATIONNOTES))
            {
                var htSituationNotes = htSituationValues.GetHashtable(SaveConstants.SAVE_SITUATIONNOTES);
                foreach (var k in htSituationNotes.Keys)
                {
                    var htThisOutput = htSituationNotes.GetHashtable(k);
                    //NOTE: distinct titles not currently used, but probably will be again
                    string title;
                    if (htThisOutput[SaveConstants.SAVE_TITLE] != null)
                        title = htThisOutput[SaveConstants.SAVE_TITLE].ToString();
                    else
                        title = "..."; //just catching possible empty descs so they don't blow up

                    
                    var notificationForSituationNote =new Notification(title,title);
                    controller.AddNote(notificationForSituationNote);
                }
            }

        }

        private void ImportSituationStoredElements(Hashtable htSituationValues, SituationController controller)
        {

            if (htSituationValues.ContainsKey(SaveConstants.SAVE_SITUATIONSTOREDELEMENTS))
            {
                var htElements = htSituationValues.GetHashtable(SaveConstants.SAVE_SITUATIONSTOREDELEMENTS);
                var elementStackSpecifications = PopulateElementStackSpecificationsList(htElements);
                foreach (var ess in elementStackSpecifications)  
                    controller.ReprovisionStoredElementStack(ess,Source.Existing());
                    
                controller.OngoingSlotsOrStorageUpdated();
            }
        }


        private void ImportSlotContents(Hashtable htSituationValues,
         SituationController controller, TabletopTokenContainer tabletop,string slotTypeKey)
        {
            //I think there's a problem here. There is an issue where we were creating ongoing slots with null GoverningSlotSpecifications for transient verbs
            //I don't know if this happens all the time? some saves? Starting slots as well but it doesn't matter?
            //(this showed up a problem where greedy slots were trying to grab from ongoing slots that didn't really exist, and threw a nullref error - I've added a guard there but the problem remains).
            if (htSituationValues.ContainsKey(slotTypeKey))
            {
                var htElements = htSituationValues.GetHashtable(slotTypeKey);
                var elementStackSpecifications = PopulateElementStackSpecificationsList(htElements);

                foreach (var ess in elementStackSpecifications.OrderBy(spec=>spec.Depth)) //this order-by is important if we're populating something with elements which create child slots -
                    //in that case we need to do it from the top down, or the slots won't be there
                {
                    var stackToPutInSlot =
                        tabletop.ReprovisionExistingElementStack(ess, Source.Existing());

                    //SaveLocationInfo for slots are recorded with an appended Guid. Everything up until the last separator is the slotId

                    //var slotId = ess.LocationInfo.Split(SaveConstants.SEPARATOR)[0];

                    int lastSeparatorPosition = ess.LocationInfo.LastIndexOf(SaveConstants.SEPARATOR);
                    var slotId = ess.LocationInfo.Substring(0, lastSeparatorPosition); //if lastseparatorposition zero-indexed is 4, length before separator - 1-indexed - is also 4


                    var slotToFill = controller.GetSlotBySaveLocationInfoPath(slotId, slotTypeKey);
                    if (slotToFill != null) //a little bit robust if a higher level element slot spec has changed between saves
                        //if the game can't find a matching slot, it'll just leave it on the desktop
                        slotToFill.AcceptStack(stackToPutInSlot, new Context(Context.ActionSource.Loading));

                    //if this was an ongoing slot, we also need to tell the situation that the slot's filled, or it will grab another

                }
            }
        }

        private List<ElementStackSpecification> PopulateElementStackSpecificationsList(Hashtable htStacks)
        {
            var elementQuantitySpecifications = new List<ElementStackSpecification>();
            foreach (var locationInfoKey in htStacks.Keys)
            {
                var htEachStack= htStacks.GetHashtable(locationInfoKey);
                

                string elementId = TryGetStringFromHashtable(htEachStack, SaveConstants.SAVE_ELEMENTID);
                int elementQuantity = GetIntFromHashtable(htEachStack, SaveConstants.SAVE_QUANTITY);
                int lifetimeRemaining = GetIntFromHashtable(htEachStack, SaveConstants.LIFETIME_REMAINING);
                bool markedForConsumption = htEachStack[SaveConstants.MARKED_FOR_CONSUMPTION].MakeBool();

                Dictionary<string, int> mutations = new Dictionary<string, int>();
                if (htEachStack.ContainsKey(SaveConstants.SAVE_MUTATIONS))
                    mutations = NoonUtility.HashtableToStringIntDictionary(
                        htEachStack.GetHashtable(SaveConstants.SAVE_MUTATIONS));

                Dictionary<string, string> illuminations = new Dictionary<string, string>();


                if (htEachStack.ContainsKey(SaveConstants.SAVE_ILLUMINATIONS))
                    illuminations = NoonUtility.HashtableToStringStringDictionary(
                        htEachStack.GetHashtable(SaveConstants.SAVE_ILLUMINATIONS));


                elementQuantitySpecifications.Add(new ElementStackSpecification(
                    elementId,
                    elementQuantity,
                    locationInfoKey.ToString(),
                    mutations,
                    illuminations,
                    lifetimeRemaining,
                    markedForConsumption));
            }
            return elementQuantitySpecifications;
        }

        private int GetQuantityFromElementHashtable(Dictionary<string, string> elementValues)
        {
            int quantity;
            var couldParse = Int32.TryParse(elementValues[SaveConstants.SAVE_QUANTITY], out quantity);
            if (!couldParse)
                throw new ArgumentException("Couldn't parse " + elementValues[SaveConstants.SAVE_QUANTITY] + " for " +
                                            elementValues[SaveConstants.SAVE_ELEMENTID] + " as a valid quantity.");
            return quantity;
        }

        private string TryGetStringFromHashtable(Hashtable ht, string key)
        {
            if (!ht.ContainsKey(key))
                return null;

            return ht[key].ToString();
        }

        //returns 0 if we can't get a value
        private int GetIntFromHashtable(Hashtable ht, string key)
        {
            if (!ht.ContainsKey(key))
                return 0;

            string jsonString = ht[key].ToString();
            int returnValue;
            if (Int32.TryParse(jsonString, out returnValue))
                return returnValue;
            else
                return 0;
        }

        private float? TryGetNullableFloatFromHashtable(Hashtable ht, string key)
        {
            if (!ht.ContainsKey(key))
                return null;

            string jsonString = ht[key].ToString();
            float returnValue;
            if(Single.TryParse(jsonString, out returnValue))
                return returnValue;
            else
                return null;
        }



    }
    
}
