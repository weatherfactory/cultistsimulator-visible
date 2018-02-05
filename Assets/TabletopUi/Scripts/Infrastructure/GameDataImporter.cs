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
            compendium.ReplaceTokens(storage);

            ImportTabletopElementStacks(tabletop, htElementStacks);

            ImportSituations(tabletop, htSituations);

            ImportDecks(storage, htDecks);

        }

        private void ImportCharacter(IGameEntityStorage storage, Hashtable htCharacter)
        {
            if(htCharacter.ContainsKey(SaveConstants.SAVE_NAME))
                storage.Name = htCharacter[SaveConstants.SAVE_NAME].ToString();

            if (htCharacter.ContainsKey(SaveConstants.SAVE_PREVIOUS_CHARACTER_NAME))
                storage.PreviousCharacterName = htCharacter[SaveConstants.SAVE_PREVIOUS_CHARACTER_NAME].ToString();

            if (htCharacter.ContainsKey(SaveConstants.SAVE_PROFESSION))
                storage.Profession = htCharacter[SaveConstants.SAVE_PROFESSION].ToString();

            if (htCharacter.ContainsKey(SaveConstants.SAVE_EXECUTIONS))
            {
                var htExecutions = htCharacter.GetHashtable(SaveConstants.SAVE_EXECUTIONS);
                foreach(var key in htExecutions.Keys)
                    storage.AddExecutionsToHistory(key.ToString(),GetIntFromHashtable(htExecutions,key.ToString()));
            }
        }


        public SavedCrossSceneState ImportCrossSceneState(Hashtable htSave)
        {
            SavedCrossSceneState state=new SavedCrossSceneState();
            
            //load current ending, if it exists
            if (htSave.ContainsKey(SaveConstants.SAVE_CURRENTENDING))
              state.CurrentEnding =compendium.GetEndingById(htSave[SaveConstants.SAVE_CURRENTENDING].ToString());

            //load legacies, if they exist
            var htLegacies = htSave.GetHashtable(SaveConstants.SAVE_AVAILABLELEGACIES);
            
            foreach (var k in htLegacies.Keys)
            {
                Legacy l = compendium.GetLegacyById(k.ToString());
                if(l!=null)
                    state.AvailableLegacies.Add(l);
            }

            //load previous character state, if it exists
            var htDefunctCharacter = htSave.GetHashtable(SaveConstants.SAVE_DEFUNCT_CHARACTER_DETAILS);
            if (htDefunctCharacter != null)
            {
                Character pc=new Character();
                pc.PreviousCharacterName = htDefunctCharacter.GetValue(SaveConstants.SAVE_NAME)
                    .ToString();
                state.DefunctCharacter = pc;
            }


            return state;
        }

        private static void ImportTabletopElementStacks(TabletopTokenContainer tabletop, Hashtable htElementStacks)
        {
            foreach (var locationInfo in htElementStacks.Keys)
            {
                var dictionaryElementStacks =
                    NoonUtility.HashtableToStringStringDictionary(htElementStacks.GetHashtable(locationInfo));

                int quantity;
                var couldParse = Int32.TryParse(dictionaryElementStacks[SaveConstants.SAVE_QUANTITY], out quantity);
                if (!couldParse)
                    throw new ArgumentException("Couldn't parse " + dictionaryElementStacks[SaveConstants.SAVE_QUANTITY] + " for " +
                                                dictionaryElementStacks[SaveConstants.SAVE_ELEMENTID] + " as a valid quantity.");

                tabletop.GetElementStacksManager()
                    .IncreaseElement(dictionaryElementStacks[SaveConstants.SAVE_ELEMENTID], quantity,Source.Existing(), locationInfo.ToString());
            }
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

                //this is pretty fragile. It assumes that the keys are contiguous integers starting at 1
                for(int i=1;i<=htEachDeck.Count;i++)
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
            List<IElementStack> outputStack = new List<IElementStack>();

            if (htSituationValues.ContainsKey(SaveConstants.SAVE_SITUATIONOUTPUTSTACKS))
            {

                var htSituationOutputStacks = htSituationValues.GetHashtable(SaveConstants.SAVE_SITUATIONOUTPUTSTACKS);

                    
                    var elementQuantitySpecifications = PopulateElementQuantitySpecificationsList(htSituationOutputStacks);
                    foreach (var eqs in elementQuantitySpecifications)
                    {
                        outputStack.Add(tabletop.ProvisionElementStack(eqs.ElementId, eqs.ElementQuantity,Source.Existing()));
                    }

            }
            return outputStack;
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
                var elementQuantitySpecifications = PopulateElementQuantitySpecificationsList(htElements);
                foreach (var eqs in elementQuantitySpecifications)   
                    controller.ModifyStoredElementStack(eqs.ElementId,eqs.ElementQuantity);                    
            }
        }


        private void ImportSlotContents(Hashtable htSituationValues,
         SituationController controller, TabletopTokenContainer tabletop,string slotTypeKey)
        {
            if (htSituationValues.ContainsKey(slotTypeKey))
            {
                var htElements = htSituationValues.GetHashtable(slotTypeKey);
                var elementQuantitySpecifications = PopulateElementQuantitySpecificationsList(htElements);

                foreach (var eqs in elementQuantitySpecifications.OrderBy(spec=>spec.Depth)) //this order-by is important if we're populating something with elements which create child slots -
                    //in that case we need to do it from the top down, or the slots won't be there
                {
                    var stackToPutInSlot =
                        tabletop.ProvisionElementStack(eqs.ElementId, eqs.ElementQuantity, Source.Existing());

                    //SaveLocationInfos are recorded with an appended Guid. We should have a SaveLocationInfo object that translates to/from safely: this is a hack.
                    var slotId = eqs.LocationInfo.Split('_')[0];


                    var slotToFill = controller.GetSlotBySaveLocationInfoPath(slotId, slotTypeKey);
                    if (slotToFill != null) //a little bit robust if a higher level element slot spec has changed between saves
                        //if the game can't find a matching slot, it'll just leave it on the desktop
                        slotToFill.AcceptStack(stackToPutInSlot);

                    //if this was an ongoing slot, we also need to tell the situation that the slot's filled, or it will grab another

                }
            }
        }

        private List<ElementQuantitySpecification> PopulateElementQuantitySpecificationsList(Hashtable htElements)
        {
            var elementQuantitySpecifications = new List<ElementQuantitySpecification>();
            foreach (var locationInfo in htElements.Keys)
            {
                var elementValues =
                    NoonUtility.HashtableToStringStringDictionary(htElements.GetHashtable(locationInfo));
                elementQuantitySpecifications.Add(new ElementQuantitySpecification(
                    elementValues[SaveConstants.SAVE_ELEMENTID],
                    GetQuantityFromElementHashtable(elementValues), locationInfo.ToString()));
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
