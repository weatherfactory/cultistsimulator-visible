using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Assets.Scripts.Application.Entities.NullEntities;
using Assets.Scripts.Application.Infrastructure.SimpleJsonGameDataImport;
using SecretHistories.Commands;
using SecretHistories.Entities; //Recipe,SlotSpecification
using SecretHistories.Enums;
using SecretHistories.Fucine;//SpherePath
using SecretHistories.NullObjects;
using SecretHistories.UI;
using SecretHistories.Commands.SituationCommands;

using OrbCreationExtensions;
using SecretHistories.Infrastructure.Persistence;
using UnityEngine;
using UnityEngine.Assertions;
using SecretHistories.Services;
using SecretHistories.Spheres;

namespace SecretHistories.Constants
{
    public class SimpleJSONGameDataImporter
    {
        private FucinePath windowSpherePath;
        private FucinePath tabletopSpherePath;


        public bool IsSavedGameActive(PetromnemeGamePersistenceProvider source)
        {
            var htSave = source.RetrieveHashedSaveFromFile();
            return htSave.ContainsKey(SaveConstants.SAVE_ELEMENTSTACKS) || htSave.ContainsKey(SaveConstants.SAVE_SITUATIONS);
        }




        public void ImportTableState(PetromnemeGamePersistenceProvider source, Sphere tabletop)
        {
            var htSave = source.RetrieveHashedSaveFromFile();

   //         windowSpherePath = new FucinePath(Watchman.Get<Compendium>().GetSingleEntity<Dictum>().DefaultWindowSpherePath);
          tabletopSpherePath = new FucinePath(Watchman.Get<Compendium>().GetSingleEntity<Dictum>().DefaultWorldSpherePath);


            var htElementStacks = htSave.GetHashtable(SaveConstants.SAVE_ELEMENTSTACKS);
            var htSituations = htSave.GetHashtable(SaveConstants.SAVE_SITUATIONS);

                ImportTabletopElementStacks(tabletop, htElementStacks);

                ImportSituations(tabletop, htSituations);
        }

        public CharacterCreationCommand ImportToCharacterCreationCommand(PetromnemeGamePersistenceProvider source)
        {
            var characterCreationCommand=new CharacterCreationCommand();
            var htSave = source.RetrieveHashedSaveFromFile();

            var htCharacter = htSave.GetHashtable(SaveConstants.SAVE_CHARACTER_DETAILS);
            if(htCharacter==null)
            {
                //we can't find a character saved in the newer format. Try looking for a defunct character with an ending saved in the older format.
                var endingTriggeredId = TryGetStringFromHashtable(htSave, SaveConstants.SAVE_CURRENTENDING);
                characterCreationCommand.EndingTriggered = Watchman.Get<Compendium>().GetEntityById<Ending>(endingTriggeredId);

                return characterCreationCommand;
            }
            

            var chosenLegacyForCharacterId =TryGetStringFromHashtable(htCharacter, SaveConstants.SAVE_ACTIVELEGACY);

            characterCreationCommand.ActiveLegacy = Watchman.Get<Compendium>().GetEntityById<Legacy>(chosenLegacyForCharacterId);


            var endingTriggeredForCharacterId =
                TryGetStringFromHashtable(htCharacter, SaveConstants.SAVE_CURRENTENDING);

            characterCreationCommand.EndingTriggered = Watchman.Get<Compendium>().GetEntityById<Ending>(endingTriggeredForCharacterId);


            
            if (htCharacter.ContainsKey(SaveConstants.SAVE_NAME))
                characterCreationCommand.Name = htCharacter[SaveConstants.SAVE_NAME].ToString();


            if (htCharacter.ContainsKey(SaveConstants.SAVE_PROFESSION))
                characterCreationCommand.Profession = htCharacter[SaveConstants.SAVE_PROFESSION].ToString();

            if (htCharacter.ContainsKey(SaveConstants.SAVE_EXECUTIONS))
            {
                var htExecutions = htCharacter.GetHashtable(SaveConstants.SAVE_EXECUTIONS);
                foreach (var key in htExecutions.Keys)
                {
                    string recipeExecutedId = key.ToString();
                    int timesExecuted = GetIntFromHashtable(htExecutions, recipeExecutedId);
                    characterCreationCommand.RecipeExecutions.Add(recipeExecutedId, timesExecuted);
                }
            }

            if (htCharacter.ContainsKey(SaveConstants.SAVE_PAST_LEVERS))
            {
                var htPastLevers = htCharacter.GetHashtable(SaveConstants.SAVE_PAST_LEVERS);
                foreach (var key in htPastLevers.Keys)
                {
                    string value = htPastLevers[key].ToString();
                    if(!string.IsNullOrEmpty(value))
                        characterCreationCommand.PreviousCharacterHistoryRecords.Add(key.ToString().ToLower(), htPastLevers[key].ToString()); //hack: we used to have camel-cased enum values as keys and they may still exist in older saves

                }
            }

            if (htCharacter.ContainsKey(SaveConstants.SAVE_FUTURE_LEVERS))
            {
                var htFutureLevers = htCharacter.GetHashtable(SaveConstants.SAVE_FUTURE_LEVERS);
                foreach (var key in htFutureLevers.Keys)
                {
                  characterCreationCommand.InProgressHistoryRecords.Add(key.ToString().ToLower(), htFutureLevers[key].ToString()); //hack: we used to have camel-cased enum values as keys  and they may still exist in older saves

                }
            }

            //TODO: deck instance import and creation
       //     var htDecks = htSave.GetHashtable(SaveConstants.SAVE_DECKS);
       //   ImportDecks(character, htDecks);

        return characterCreationCommand;
        }
        

        private void ImportTabletopElementStacks(Sphere tabletop, Hashtable htElementStacks)
        {
            var elementStackSpecifications = PopulateElementStackSpecificationsList(htElementStacks);

            foreach (var ess in elementStackSpecifications)
            {
           //     tabletop.ProvisionStackFromCommand(ess);
            }

        }

        private void ImportDecks(Character character, Hashtable htDeckInstances)
        {
            foreach (var k in htDeckInstances.Keys)
            {
                var htEachDeck = htDeckInstances.GetHashtable(k);

                DeckSpec spec = Watchman.Get<Compendium>().GetEntityById<DeckSpec>(k.ToString());

                if (spec == null)
                    NoonUtility.Log("no deckspec found for saved deckinstance " + k.ToString());
                else
                {
                  //  character.UpdateDeckInstanceFromSave(spec, htEachDeck);
                }
            }
        
        }

        private void ImportSituations(Sphere tabletop, Hashtable htSituations)
        {

            foreach (var locationInfo in htSituations.Keys)
            {
                var htSituationValues =htSituations.GetHashtable(locationInfo);
                
                var verb = GetSituationVerb(htSituationValues);
                var recipe = GetSituationRecipe(htSituationValues, verb);
                var situationState= GetSituationState(htSituationValues);

                var command = SetupSituationTokenCreationCommand(verb, recipe, situationState, htSituationValues, locationInfo);

                var situationToken= command.Execute(new Context(Context.ActionSource.Loading),Watchman.Get<HornedAxe>().GetDefaultSphere());
                var situation=situationToken.Payload as Situation;
                

                ImportSlotContents(situation,htSituationValues,  SaveConstants.SAVE_STARTINGSLOTELEMENTS);
                ImportSlotContents(situation, htSituationValues,  SaveConstants.SAVE_ONGOINGSLOTELEMENTS);
                ImportSituationStoredElements(htSituationValues, situation);

                ImportOutputs(htSituationValues, situation, tabletop);

                //this should happen last, because adding those stacks above can overwrite notes
                ImportSituationNotes(htSituationValues, situation);
               

            }
        }

        private TokenCreationCommand SetupSituationTokenCreationCommand(Verb verb, Recipe recipe, StateEnum situationState,
            Hashtable htSituationValues, object locationInfo)
        {
            var situationCreationCommand = new SituationCreationCommand(verb.Id).WithRecipeId(recipe.Id).AlreadyInState(situationState);
            
            situationCreationCommand.TimeRemaining = TryGetNullableFloatFromHashtable(htSituationValues, SaveConstants.SAVE_TIMEREMAINING) ??  0;
            situationCreationCommand.OverrideTitle = TryGetStringFromHashtable(htSituationValues, SaveConstants.SAVE_TITLE);

            string simplifiedSituationPath;
            TokenLocation tokenLocation;

            string[] simplifiedSituationPathParts = locationInfo.ToString().Split(FucinePath.SPHERE);
            if (simplifiedSituationPathParts.Length != 3)
            {
                NoonUtility.LogWarning(
                    $"We can't parse a situation locationinfo: {locationInfo}. So we're just picking the beginning of it to use as the situation path.");
                simplifiedSituationPath = simplifiedSituationPathParts[0];
                tokenLocation = new TokenLocation(0, 0, 0, tabletopSpherePath);
            }
            else
            {
                simplifiedSituationPath = simplifiedSituationPathParts[2];
                float.TryParse(simplifiedSituationPathParts[0], out float anchorPosX);
                float.TryParse(simplifiedSituationPathParts[1], out float anchorPosY);
                tokenLocation = new TokenLocation(anchorPosX, anchorPosY, 0, tabletopSpherePath);
            }



            situationCreationCommand.IsOpen = htSituationValues[SaveConstants.SAVE_SITUATION_WINDOW_OPEN].MakeBool();
            
            
            var verbSlotsCommand = new PopulateDominionCommand(SituationDominionEnum.VerbThresholds.ToString(),verb.Thresholds);
            situationCreationCommand.CommandQueue.AddCommand(verbSlotsCommand);


            var recipeSlotSpecs = SimpleJsonSlotImporter.ImportSituationOngoingSlotSpecs(htSituationValues, recipe.Slots);
            var recipeSlotsCommand = new PopulateDominionCommand(SituationDominionEnum.RecipeThresholds.ToString(),recipeSlotSpecs);
            situationCreationCommand.CommandQueue.AddCommand(recipeSlotsCommand);
            
            var tokenCreationCommand=new TokenCreationCommand(situationCreationCommand, tokenLocation);

            return tokenCreationCommand;
        }


        private static StateEnum GetSituationState(Hashtable htSituationValues)
        {
            return (StateEnum)Enum.Parse(typeof(StateEnum), htSituationValues[SaveConstants.SAVE_SITUATIONSTATE].ToString());
        }

        private static Verb GetSituationVerb(Hashtable htSituationValues)
        {
            string verbId = htSituationValues[SaveConstants.SAVE_VERBID].ToString();
            Verb situationVerb = Watchman.Get<Compendium>().GetEntityById<Verb>(verbId);
            if (situationVerb == null)
                situationVerb = NullVerb.Create();
            return situationVerb;
        }

        private Recipe GetSituationRecipe(Hashtable htSituationValues, Verb situationVerb)
        {
            string recipeId = TryGetStringFromHashtable(htSituationValues, SaveConstants.SAVE_RECIPEID);
            var recipe = Watchman.Get<Compendium>().GetEntityById<Recipe>(recipeId);
            if (recipe == null)
                recipe = Recipe.CreateSpontaneousHintRecipe(situationVerb);
            return recipe;
        }

        private void ImportSlotContents(Situation situation,Hashtable htSituationValues, string slotTypeKey)
        {
            //I think there's a problem here. There is an issue where we were creating ongoing slots with null GoverningSlotSpecifications for transient verbs
            ////I don't know if this happens all the time? some saves? Starting slots as well but it doesn't matter?
            ////(this showed up a problem where greedy slots were trying to grab from ongoing slots that didn't really exist, and threw a nullref error - I've added a guard there but the problem remains).
            if (htSituationValues.ContainsKey(slotTypeKey))
            {
                var htElements = htSituationValues.GetHashtable(slotTypeKey);
                var elementStackSpecifications = PopulateElementStackSpecificationsList(htElements);

                foreach (var ess in elementStackSpecifications.OrderBy(spec => spec.Depth)) //this order-by is important if we're populating something with elements which create child slots -
                                                                                            //in that case we need to do it from the top down, or the slots won't be there
                {
           //         var slotPath = situation.CachedParentPath.AppendPath(ess.LocationInfo.Split(FucinePath.SPHERE)[0]);
          //          var slot = Watchman.Get<HornedAxe>().GetSphereByPath(slotPath);
              //      slot.ProvisionStackFromCommand(ess);

                }
            }
        }

        private void ImportOutputs(Hashtable htSituationValues, Situation situation, Sphere tabletop)
        {
         var outputStacks=ImportOutputStacks(htSituationValues, tabletop);
            situation.AcceptTokens(SphereCategory.Output,outputStacks);

        }

        private List<Token> ImportOutputStacks(Hashtable htSituationValues, Sphere tabletop)
        {
            List<Token> outputStacks = new List<Token>();

            if (htSituationValues.ContainsKey(SaveConstants.SAVE_SITUATIONOUTPUTSTACKS))
            {

                var htSituationOutputStacks = htSituationValues.GetHashtable(SaveConstants.SAVE_SITUATIONOUTPUTSTACKS);

                    
                    var stackSpecification = PopulateElementStackSpecificationsList(htSituationOutputStacks);
                    foreach (var ess in stackSpecification)
                    {
                //        outputStacks.Add(tabletop.ProvisionStackFromCommand(ess));
                    }

            }
            return outputStacks;
        }

        private void ImportSituationNotes(Hashtable htSituationValues,Situation situation)
        {
            if (htSituationValues.ContainsKey(SaveConstants.SAVE_SITUATIONNOTES))
            {
                SortedDictionary<int, Notification> notes=new SortedDictionary<int, Notification>();

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

                    if(int.TryParse(k.ToString(),out int order))
                        notes.Add(order,notificationForSituationNote);
                    else
                       notes.Add(notes.Count,notificationForSituationNote);
                
                }
                

            }

        }

        private void ImportSituationStoredElements(Hashtable htSituationValues, Situation situation)
        {

            if (htSituationValues.ContainsKey(SaveConstants.SAVE_SITUATIONSTOREDELEMENTS))
            {
                var htElements = htSituationValues.GetHashtable(SaveConstants.SAVE_SITUATIONSTOREDELEMENTS);
                var elementStackSpecifications = PopulateElementStackSpecificationsList(htElements);
                foreach (var ess in elementStackSpecifications)
                {
                   // var stackToStore=Watchman.Get<Limbo>().ProvisionStackFromCommand(ess);


                 //   situation.AcceptToken(SphereCategory.SituationStorage, stackToStore,
                //        new Context(Context.ActionSource.Loading));
                }


            }
        }




        private List<ElementStackSpecification_ForSimpleJSONDataImport> PopulateElementStackSpecificationsList(Hashtable htStacks)
        {
            var stackCreationCommand = new List<ElementStackSpecification_ForSimpleJSONDataImport>();
            foreach (var locationInfoKey in htStacks.Keys)
            {
                var htEachStack= htStacks.GetHashtable(locationInfoKey);
                

                string elementId = TryGetStringFromHashtable(htEachStack, SaveConstants.SAVE_ELEMENTID);
                int elementQuantity = GetIntFromHashtable(htEachStack, SaveConstants.SAVE_QUANTITY);
                int lifetimeRemaining = GetIntFromHashtable(htEachStack, SaveConstants.LIFETIME_REMAINING);
				float? posx = TryGetNullableFloatFromHashtable(htEachStack, SaveConstants.SAVE_LASTTABLEPOS_X);
				float? posy = TryGetNullableFloatFromHashtable(htEachStack, SaveConstants.SAVE_LASTTABLEPOS_Y);
				Vector3 lasttablepos = new Vector2( posx.HasValue ? posx.Value : 0.0f, posy.HasValue ? posy.Value : 0.0f );
				//Debug.Log("Loaded lastTablePos " + lasttablepos.x + ", " + lasttablepos.y);

                Dictionary<string, int> mutations = new Dictionary<string, int>();
                if (htEachStack.ContainsKey(SaveConstants.SAVE_MUTATIONS))
                    mutations = NoonUtility.HashtableToStringIntDictionary(
                        htEachStack.GetHashtable(SaveConstants.SAVE_MUTATIONS));

                Dictionary<string, string> illuminations = new Dictionary<string, string>();


                if (htEachStack.ContainsKey(SaveConstants.SAVE_ILLUMINATIONS))
                    illuminations = NoonUtility.HashtableToStringStringDictionary(
                        htEachStack.GetHashtable(SaveConstants.SAVE_ILLUMINATIONS));


                TokenLocation stackLocation=new TokenLocation(lasttablepos,tabletopSpherePath);

                Context context=new Context(Context.ActionSource.Loading, stackLocation);
                
                stackCreationCommand.Add(new ElementStackSpecification_ForSimpleJSONDataImport(
                    elementId,
                    elementQuantity,
                    locationInfoKey.ToString(),
                    mutations,
                    illuminations,
                    lifetimeRemaining,
                    context));
            }
            return stackCreationCommand;
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
