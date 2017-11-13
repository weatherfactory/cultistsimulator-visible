﻿using System;
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

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public interface IGameDataImporter
    {
        void ImportSavedGameToState(TabletopContainer tabletopContainer, IGameEntityStorage storage, Hashtable htSave);
        SavedCrossSceneState ImportCrossSceneState(Hashtable htSave);
    }

    public class GameDataImporter : IGameDataImporter
    {
        private ICompendium compendium;

        
        public GameDataImporter(ICompendium compendium)
        {
            this.compendium = compendium;
        }



        public void ImportSavedGameToState(TabletopContainer tabletopContainer,IGameEntityStorage storage, Hashtable htSave)
        {
            var htElementStacks = htSave.GetHashtable(SaveConstants.SAVE_ELEMENTSTACKS);
            var htSituations = htSave.GetHashtable(SaveConstants.SAVE_SITUATIONS);
            var htDecks = htSave.GetHashtable(SaveConstants.SAVE_DECKS);

            ImportTabletopElementStacks(tabletopContainer, htElementStacks);

            ImportSituations(tabletopContainer, htSituations);

            ImportDecks(storage, htDecks);

        }



        public SavedCrossSceneState ImportCrossSceneState(Hashtable htSave)
        {
            SavedCrossSceneState state=new SavedCrossSceneState();
            if (htSave.ContainsKey(SaveConstants.SAVE_CURRENTENDING))
              state.CurrentEnding =compendium.GetEndingById(htSave[SaveConstants.SAVE_CURRENTENDING].ToString());

            var htLegacies = htSave.GetHashtable(SaveConstants.SAVE_AVAILABLELEGACIES);
            
            foreach (var k in htLegacies.Keys)
            {
                Legacy l = compendium.GetLegacyById(k.ToString());
                if(l!=null)
                    state.AvailableLegacies.Add(l);

            }

            return state;
        }

        private static void ImportTabletopElementStacks(TabletopContainer tabletopContainer, Hashtable htElementStacks)
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

                tabletopContainer.GetElementStacksManager()
                    .IncreaseElement(dictionaryElementStacks[SaveConstants.SAVE_ELEMENTID], quantity,Source.Existing(), locationInfo.ToString());
            }
        }

        private void ImportDecks(IGameEntityStorage storage, Hashtable htDeckInstances)
        {
            foreach (var k in htDeckInstances.Keys)
            {
                var htEachDeck = htDeckInstances.GetHashtable(k);

                IDeckSpec spec = compendium.GetDeckSpecById(k.ToString());
                IDeckInstance deckInstance =  new DeckInstance(spec);

                //this is pretty fragile. It assumes that the keys are contiguous integers starting at 1
                for(int i=1;i<=htEachDeck.Count;i++)
                    deckInstance.Add(htEachDeck[i.ToString()].ToString());

                storage.DeckInstances.Add(deckInstance);
            }
        }

        private void ImportSituations(TabletopContainer tabletopContainer, Hashtable htSituations)
        {
            foreach (var locationInfo in htSituations.Keys)
            {
                var htSituationValues =htSituations.GetHashtable(locationInfo);

                IVerb situationVerb = compendium.GetVerbById(htSituationValues[SaveConstants.SAVE_VERBID].ToString());

                string recipeId = TryGetStringFromHashtable(htSituationValues, SaveConstants.SAVE_RECIPEID);
                var recipe = compendium.GetRecipeById(recipeId);

                var command = new SituationCreationCommand(situationVerb, recipe, (SituationState)Enum.Parse(typeof(SituationState), htSituationValues[SaveConstants.SAVE_SITUATIONSTATE].ToString()));
                command.TimeRemaining = TryGetNullableFloatFromHashtable(htSituationValues, SaveConstants.SAVE_TIMEREMAINING);

                command.OverrideTitle = TryGetStringFromHashtable(htSituationValues, SaveConstants.SAVE_TITLE);
                command.CompletionCount = GetIntFromHashtable(htSituationValues, SaveConstants.SAVE_COMPLETIONCOUNT);


                var situationAnchor= tabletopContainer.CreateSituation(command, locationInfo.ToString());
                var situationController = situationAnchor.SituationController;

                ImportSituationNotes(htSituationValues,situationController);

                ImportSlotContents(htSituationValues, situationController, tabletopContainer, SaveConstants.SAVE_STARTINGSLOTELEMENTS);
                ImportSlotContents(htSituationValues, situationController, tabletopContainer, SaveConstants.SAVE_ONGOINGSLOTELEMENTS);

                ImportSituationStoredElements(htSituationValues, situationController);
                ImportOutputs(htSituationValues, situationController, tabletopContainer);


                
            }
        }

        private void ImportOutputs(Hashtable htSituationValues, SituationController situationController, TabletopContainer tabletopContainer)
        {
         var outputStacks=ImportOutputStacks(htSituationValues, tabletopContainer);
            situationController.SetOutput(outputStacks);

        }

        private List<IElementStack> ImportOutputStacks(Hashtable htSituationValues, TabletopContainer tabletopContainer)
        {
            List<IElementStack> outputStack = new List<IElementStack>();

            if (htSituationValues.ContainsKey(SaveConstants.SAVE_SITUATIONOUTPUTSTACKS))
            {
                //this is probably one more loop than I need.
                var htSituationOutputStacks = htSituationValues.GetHashtable(SaveConstants.SAVE_SITUATIONOUTPUTSTACKS);
               // foreach (var k in htSituationOutputStacks.Keys)
               // {
                   // var htThisOutputStack = htSituationOutputStacks.GetHashtable(k);
                   // var htOutputElements = htThisOutputStack.GetHashtable(SaveConstants.SAVE_SITUATIONNOTES);
                    
                    var elementQuantitySpecifications = PopulateElementQuantitySpecificationsList(htSituationOutputStacks);
                    foreach (var eqs in elementQuantitySpecifications)
                    {
                        outputStack.Add(tabletopContainer.GetTokenTransformWrapper().ProvisionElementStack(eqs.ElementId, eqs.ElementQuantity,Source.Existing()));
                    }

               // }
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
                    //NOTE: titles not currently used, but probably will be again
                    var notificationForSituationNote =new Notification(htThisOutput[SaveConstants.SAVE_TITLE].ToString(), htThisOutput[SaveConstants.SAVE_TITLE].ToString());
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
         SituationController controller, TabletopContainer tabletopContainer,string slotTypeKey)
        {
            if (htSituationValues.ContainsKey(slotTypeKey))
            {
                var htElements = htSituationValues.GetHashtable(slotTypeKey);
                var elementQuantitySpecifications = PopulateElementQuantitySpecificationsList(htElements);

                foreach (var eqs in elementQuantitySpecifications.OrderBy(spec=>spec.Depth)) //this order-by is important if we're populating something with elements which create child slots -
                    //in that case we need to do it from the top down, or the slots won't be there
                {
                    var stackToPutInSlot =
                        tabletopContainer.GetTokenTransformWrapper()
                            .ProvisionElementStack(eqs.ElementId, eqs.ElementQuantity, Source.Existing());
                    var slotToFill = controller.GetSlotBySaveLocationInfoPath(eqs.LocationInfo, slotTypeKey);
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
