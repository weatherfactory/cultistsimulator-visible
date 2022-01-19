using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Commands;
using SecretHistories.Entities; //Recipe,SlotSpecification
using SecretHistories.Enums;
using SecretHistories.Fucine; //SpherePath
using SecretHistories.NullObjects;
using SecretHistories.UI;
using SecretHistories.Commands.SituationCommands;
using OrbCreationExtensions;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Commands.SituationCommands;
using SecretHistories.Constants;
using SecretHistories.Infrastructure.Persistence;
using UnityEngine;
using UnityEngine.Assertions;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.States.TokenStates;

/// <summary>
/// Step-by-step conversion from simplejson data format into encaustery commands
/// </summary>
public class PetromnemeImporter
{
    private const string CLASSIC_DROPZONE_ELEMENT_ID = "dropzone";
    private const string CLASSIC_TABLETOP_SPHERE__ID = "tabletop";

    private const char CLASSIC_SITUATION_PATH_PART_SEPARATOR = '_';


    public bool IsSavedGameActive(PetromnemeGamePersistenceProvider source)
    {
        var htSave = source.RetrieveHashedSaveFromFile();
        return htSave.ContainsKey(SaveConstants.SAVE_ELEMENTSTACKS) ||
               htSave.ContainsKey(SaveConstants.SAVE_SITUATIONS);
    }


    public RootPopulationCommand ImportTableState(PetromnemeGamePersistenceProvider source, Legacy currentLegacy)
    {
        var rootCommand = new RootPopulationCommand();
        var tabletopSphereCreationCommand = RootPopulationCommand.DefaultSphereCreationCommand();
        rootCommand.Spheres.Add(tabletopSphereCreationCommand);

        var htSave = source.RetrieveHashedSaveFromFile();
        var htDecks = htSave.GetHashtable(SaveConstants.SAVE_DECKS);
        AddDecksToRootCommand(rootCommand, htDecks, currentLegacy);


        //get all tabletop element stacks
        var htElementStacks = htSave.GetHashtable(SaveConstants.SAVE_ELEMENTSTACKS);
        AddElementsFromHashTableToSphereCommand(tabletopSphereCreationCommand, htElementStacks);

        //get all tabletop  situation tokens
        var htSituations = htSave.GetHashtable(SaveConstants.SAVE_SITUATIONS);
          AddSituationsToTabletopCommand(tabletopSphereCreationCommand, htSituations);


        //big old fudge to get the new verb dropzone in
        var tabletopSpherePath= Watchman.Get<Compendium>().GetSingleEntity<Dictum>().DefaultWorldSpherePath;
        
        var situationDropzoneLocation = TokenLocation.Default(tabletopSpherePath);
        
          var situationDropzoneCreationCommand = new DropzoneCreationCommand(nameof(Situation).ToString());
          var situationDropzoneTokenCreationCommand = new TokenCreationCommand(situationDropzoneCreationCommand, situationDropzoneLocation);
          tabletopSphereCreationCommand.Tokens.Add(situationDropzoneTokenCreationCommand);
          NoonUtility.Log($"PETRO: Creating new verb dropzone and placing it on the tabletop");


        return rootCommand;
    }

    public CharacterCreationCommand ImportToCharacterCreationCommand(PetromnemeGamePersistenceProvider source)
    {
        var characterCreationCommand = new CharacterCreationCommand();
        var htSave = source.RetrieveHashedSaveFromFile();

        var htCharacter = htSave.GetHashtable(SaveConstants.SAVE_CHARACTER_DETAILS);
        if (htCharacter == null)
        {
            NoonUtility.Log(
                "PETRO: We can't find a character saved in the newer petromneme format. Try looking for a defunct character with an ending saved in the *archaic* petromneme format.");
            var endingTriggeredId = TryGetStringFromHashtable(htSave, SaveConstants.SAVE_CURRENTENDING);
            if (!string.IsNullOrEmpty(endingTriggeredId))
                NoonUtility.Log($"PETRO: Found ending triggered id {endingTriggeredId}");
            characterCreationCommand.EndingTriggeredId = endingTriggeredId;
            NoonUtility.Log(
                "PETRO: Returning character creation command with just 'endingtriggered' populated. That's all we get.");
            return characterCreationCommand;
        }

        var chosenLegacyForCharacterId = TryGetStringFromHashtable(htCharacter, SaveConstants.SAVE_ACTIVELEGACY);
        if (!string.IsNullOrEmpty(chosenLegacyForCharacterId))
            NoonUtility.Log($"PETRO: Found chosenLegacyForCharacterId {chosenLegacyForCharacterId}");

        characterCreationCommand.ActiveLegacyId = chosenLegacyForCharacterId;
        NoonUtility.Log(
            $"PETRO: Added ActiveLegacy {characterCreationCommand.ActiveLegacyId} to char creation command");


        var activeLegacy = Watchman.Get<Compendium>().GetEntityById<Legacy>(characterCreationCommand.ActiveLegacyId);

        characterCreationCommand.Profession = activeLegacy.Label;
            NoonUtility.Log(
                $"PETRO: Adding char profession {characterCreationCommand.Profession} to char creation command, based on active legacy label");


            var endingTriggeredForCharacterId =
            TryGetStringFromHashtable(htCharacter, SaveConstants.SAVE_CURRENTENDING);
        var EndingTriggered =
            Watchman.Get<Compendium>().GetEntityById<Ending>(endingTriggeredForCharacterId);

        if (EndingTriggered.IsValid())
        {
            characterCreationCommand.EndingTriggeredId = EndingTriggered.Id;
            NoonUtility.Log(
                $"PETRO: Added Ending {EndingTriggered.Id} to char creation command");
        }
        else
        {
            NoonUtility.Log($"PETRO: No triggered ending found; not adding one to char creation command");
        }

        if (htCharacter.ContainsKey(SaveConstants.SAVE_NAME))
        {
            characterCreationCommand.Name = htCharacter[SaveConstants.SAVE_NAME].ToString();
            NoonUtility.Log($"PETRO: Adding char name {characterCreationCommand.Name} to char creation command");
        }


        if (htCharacter.ContainsKey(SaveConstants.SAVE_EXECUTIONS))
        {
            var htExecutions = htCharacter.GetHashtable(SaveConstants.SAVE_EXECUTIONS);
            foreach (var key in htExecutions.Keys)
            {
                var recipeExecutedId = key.ToString();
                var timesExecuted = GetIntFromHashtable(htExecutions, recipeExecutedId);
                characterCreationCommand.RecipeExecutions.Add(recipeExecutedId, timesExecuted);
            }

            NoonUtility.Log($"PETRO: Added {htExecutions.Keys.Count} past recipe executions to char creation command");
        }

        if (htCharacter.ContainsKey(SaveConstants.SAVE_PAST_LEVERS))
        {
            var htPastLevers = htCharacter.GetHashtable(SaveConstants.SAVE_PAST_LEVERS);
            foreach (var key in htPastLevers.Keys)
            {
                var value = htPastLevers[key].ToString();
                if (!string.IsNullOrEmpty(value))
                    characterCreationCommand.PreviousCharacterHistoryRecords.Add(key.ToString().ToLower(),
                        htPastLevers[key]
                            .ToString()); //hack: we used to have camel-cased enum values as keys and they may still exist in older saves
            }

            NoonUtility.Log($"PETRO: Added {htPastLevers.Keys.Count} past levers to char creation command");
        }

        if (htCharacter.ContainsKey(SaveConstants.SAVE_FUTURE_LEVERS))
        {
            var htFutureLevers = htCharacter.GetHashtable(SaveConstants.SAVE_FUTURE_LEVERS);
            foreach (var key in htFutureLevers.Keys)
                characterCreationCommand.InProgressHistoryRecords.Add(key.ToString().ToLower(),
                    htFutureLevers[key]
                        .ToString()); //hack: we used to have camel-cased enum values as keys  and they may still exist in older saves

            NoonUtility.Log($"PETRO: Added {htFutureLevers.Keys.Count} future levers to char creation command");
        }


        return characterCreationCommand;
    }


    private void AddElementsFromHashTableToSphereCommand(SphereCreationCommand sphereCommand, Hashtable htElementStacks)
    {
        var tabletopElementStacks = 0;

        foreach (var locationInfoKey in htElementStacks.Keys)
        {
            var htEachStack = htElementStacks.GetHashtable(locationInfoKey);
            ITokenPayloadCreationCommand tokenPayloadCreationCommand;


            var elementId = TryGetStringFromHashtable(htEachStack, SaveConstants.SAVE_ELEMENTID);
            if (elementId == CLASSIC_DROPZONE_ELEMENT_ID) //filter dropzone out of element stacks
            {
                var elementDropzoneCreationCommand = new DropzoneCreationCommand(nameof(ElementStack));
                
                tokenPayloadCreationCommand= elementDropzoneCreationCommand;
                
                NoonUtility.Log($"PETRO: Adding new element dropzone to the tabletop.");
            }

            else

            {
                var elementQuantity = GetIntFromHashtable(htEachStack, SaveConstants.SAVE_QUANTITY);
                var elementStackCreationCommand = new ElementStackCreationCommand(elementId, elementQuantity);

                var lifetimeRemaining = GetIntFromHashtable(htEachStack, SaveConstants.LIFETIME_REMAINING);
                elementStackCreationCommand.LifetimeRemaining = lifetimeRemaining;


                if (htEachStack.ContainsKey(SaveConstants.SAVE_MUTATIONS))
                    elementStackCreationCommand.Mutations = NoonUtility.HashtableToStringIntDictionary(
                        htEachStack.GetHashtable(SaveConstants.SAVE_MUTATIONS));


                if (htEachStack.ContainsKey(SaveConstants.SAVE_ILLUMINATIONS))
                    elementStackCreationCommand.Illuminations = NoonUtility.HashtableToStringStringDictionary(
                        htEachStack.GetHashtable(SaveConstants.SAVE_ILLUMINATIONS));
                NoonUtility.Log($"PETRO: Adding {elementId} ({elementQuantity}) to the tabletop.");
                tokenPayloadCreationCommand = elementStackCreationCommand;
            }

            var posx = TryGetNullableFloatFromHashtable(htEachStack, SaveConstants.SAVE_LASTTABLEPOS_X);
            var posy = TryGetNullableFloatFromHashtable(htEachStack, SaveConstants.SAVE_LASTTABLEPOS_Y);
            Vector3 lasttablepos = new Vector2(posx.HasValue ? posx.Value : 0.0f, posy.HasValue ? posy.Value : 0.0f);


            var stackTokenLocation =
                new TokenLocation(lasttablepos,
                    FucinePath.Current()); //won't be en route sphere path cos that was not thought on in these times
            //the 'current' makes me a little nervous, because at the time of writing there is still some murkiness:
            //execute a spherecreationcommand with child tokencreationcommands, and the sphere will be created and then specified as the sphere location
            //so the path in the tokenlocation is actually irrelevant. Probably it *should* be 'current', but we don't cater helpfully for this.


            var tokenCreationCommand = new TokenCreationCommand(tokenPayloadCreationCommand, stackTokenLocation);
            if(sphereCommand.GoverningSphereSpec.Id ==CLASSIC_TABLETOP_SPHERE__ID)
                tokenCreationCommand.CurrentState=new PlacedAssertivelyBySystemState();

            sphereCommand.Tokens.Add(tokenCreationCommand);
            tabletopElementStacks++;
        

        }

        NoonUtility.Log($"PETRO: Adding {tabletopElementStacks} element stacks to the tabletop.");
    }

    private void AddDecksToRootCommand(RootPopulationCommand rootCommand, Hashtable htDeckInstances, Legacy currentLegacy)
    {
        NoonUtility.Log("PETRO: Adding DealersTable dominion command, so we have something nice to put cards on.");
        rootCommand.DealersTable = new PopulateDominionCommand();
        var allDeckSpecs = Watchman.Get<Compendium>().GetEntitiesAsAlphabetisedList<DeckSpec>();
        var deckSpecsImported = 0;
        var deckInstancesImported = 0;
        foreach (var deckSpec in allDeckSpecs)
            if (string.IsNullOrEmpty(deckSpec.ForLegacyFamily) || currentLegacy.Family == deckSpec.ForLegacyFamily)
            {
                var drawSphereCommand = CreateDrawSphereCommand(deckSpec);
                rootCommand.DealersTable.Spheres.Add(drawSphereCommand);


                var forbiddenCardsSphereCommand = CreateForbiddenCardsSphereCommand(deckSpec);
                rootCommand.DealersTable.Spheres.Add(forbiddenCardsSphereCommand);

                var htThisDeckInstance = htDeckInstances.GetHashtable(deckSpec.Id);

                if (htThisDeckInstance != null)
                {
                    ImportDeckInstance(htThisDeckInstance, forbiddenCardsSphereCommand, drawSphereCommand);
                    deckInstancesImported++;
                }


                deckSpecsImported++;
            }

        NoonUtility.Log($"PETRO: Added {deckInstancesImported} deck instances for {deckSpecsImported} deckspecs.");
    }

    private static void ImportDeckInstance(Hashtable htThisDeckInstance,
        SphereCreationCommand forbiddenCardsSphereCommand,
        SphereCreationCommand drawSphereCommand)
    {
        foreach (var cardKey in htThisDeckInstance.Keys)
            if (cardKey.ToString() == SaveConstants.SAVE_ELIMINATEDCARDS)
            {
                var htEliminatedCardsForThisDeckInstance =
                    htThisDeckInstance.GetHashtable(SaveConstants.SAVE_ELIMINATEDCARDS);
                if (htEliminatedCardsForThisDeckInstance != null)
                    foreach (var eliminatedk in htEliminatedCardsForThisDeckInstance.Keys)
                        forbiddenCardsSphereCommand.Tokens.Add(
                            new TokenCreationCommand().WithElementStack(
                                htEliminatedCardsForThisDeckInstance[eliminatedk].ToString(), 1));
            }
            else
            {
                var cardElementId = htThisDeckInstance[cardKey].ToString();
                drawSphereCommand.Tokens.Add(
                    new TokenCreationCommand().WithElementStack(cardElementId, 1));
            }
    }

    private static SphereCreationCommand CreateForbiddenCardsSphereCommand(DeckSpec deckSpec)
    {
        var forbiddenCardsSphereSpec = new SphereSpec(typeof(ForbiddenPile), $"{deckSpec.Id}_forbidden");
        forbiddenCardsSphereSpec.ActionId = deckSpec.Id;
        var discardSphereCommand = new SphereCreationCommand(forbiddenCardsSphereSpec);
        return discardSphereCommand;
    }

    private static SphereCreationCommand CreateDrawSphereCommand(DeckSpec deckSpec)
    {
        var drawSphereSpec = new SphereSpec(typeof(DrawPile), $"{deckSpec.Id}_draw");
        drawSphereSpec.ActionId = deckSpec.Id;
        var drawSphereCommand = new SphereCreationCommand(drawSphereSpec);
        return drawSphereCommand;
    }

    private void AddSituationsToTabletopCommand(SphereCreationCommand tabletopCommand, Hashtable htSituations)
    {
        var situationTokensImported = 0;
        
        foreach (var locationInfo in htSituations.Keys)
        {
            var htSituationValues = htSituations.GetHashtable(locationInfo);

            var verb = GetSituationVerb(htSituationValues); //TODO: can this be null? What happens with base and transient verbs?
            var recipe = GetSituationRecipe(htSituationValues, verb);
            var situationState = GetSituationState(htSituationValues);

            var situationTokenCommand =
                SetupSituationTokenCreationCommand(verb, recipe, situationState, htSituationValues, locationInfo);

            var situationCommand = situationTokenCommand.Payload as SituationCreationCommand;
            
            RescueElementStacksThatWouldBeInVerbSlots(tabletopCommand, htSituationValues);
            AddElementStacksFromSituationValuesHashTableToDominionSphere(situationCommand, htSituationValues,SaveConstants.SAVE_ONGOINGSLOTELEMENTS,SituationDominionEnum.RecipeThresholds);
            AddElementStacksFromSituationValuesHashTableToDominionSphere(situationCommand, htSituationValues, SaveConstants.SAVE_SITUATIONSTOREDELEMENTS, SituationDominionEnum.Storage);
            AddElementStacksFromSituationValuesHashTableToDominionSphere(situationCommand, htSituationValues, SaveConstants.SAVE_SITUATIONOUTPUTSTACKS, SituationDominionEnum.Output);
  
            //this should happen last, because adding those stacks above can overwrite notes
            AddNotesToSituationCommand(situationCommand,htSituationValues,verb);

            tabletopCommand.Tokens.Add(situationTokenCommand);
            situationTokensImported++;
        NoonUtility.Log($"PETRO: Adding situation {verb.Id} ({recipe.Id}), currently {situationState}, to the tabletop.");


        }

        NoonUtility.Log($"PETRO: Adding {situationTokensImported} situations to the tabletop.");

    }

    private void RescueElementStacksThatWouldBeInVerbSlots(SphereCreationCommand tabletopCommand,
        Hashtable htSituationValues)
    {
        //moving elements in verb slots - particularly child slots - into the new system is a pain in the bum. So we just dump them on the table.
        if (htSituationValues.ContainsKey(SaveConstants.SAVE_STARTINGSLOTELEMENTS))
        {
            var htElementsRescuedFromVerbSlots = htSituationValues.GetHashtable(SaveConstants.SAVE_STARTINGSLOTELEMENTS);
            AddElementsFromHashTableToSphereCommand(tabletopCommand,htElementsRescuedFromVerbSlots);
        }
    }

    private TokenCreationCommand SetupSituationTokenCreationCommand(Verb verb, Recipe recipe, StateEnum situationState,
        Hashtable htSituationValues, object locationInfo)
    {
        var situationCreationCommand =
            new SituationCreationCommand(verb.Id).WithRecipeId(recipe.Id).AlreadyInState(situationState);

        situationCreationCommand.TimeRemaining =
            TryGetNullableFloatFromHashtable(htSituationValues, SaveConstants.SAVE_TIMEREMAINING) ?? 0;
        //   situationCreationCommand.OverrideTitle = TryGetStringFromHashtable(htSituationValues, SaveConstants.SAVE_TITLE);

        TokenLocation tokenLocation;

        //get x, y vector from old weird situation notation into 
        var simplifiedSituationPathParts = locationInfo.ToString().Split(CLASSIC_SITUATION_PATH_PART_SEPARATOR);
        if (simplifiedSituationPathParts.Length != 3)
        {
            NoonUtility.LogWarning(
                $"We can't parse a situation locationinfo: {locationInfo}. So we're just picking the beginning of it to use as the situation path.");
            tokenLocation = new TokenLocation(0, 0, 0, FucinePath.Current());
        }
        else
        {
            
            float.TryParse(simplifiedSituationPathParts[0], out var anchorPosX);
            float.TryParse(simplifiedSituationPathParts[1], out var anchorPosY);
            tokenLocation = new TokenLocation(anchorPosX, anchorPosY, 0, FucinePath.Current());
        }

        
        situationCreationCommand.IsOpen = false; //No open situations. This means we don't have to worry about the gummy complexities of importing elementstacks into verb slots and, more importantly, child slots: we just dump 'em on the table.

        //verb slots based on current situation verb. No child slots, because there are no tokens in the verb slot!
        var verbSlotsCommand =
            new PopulateDominionCommand(SituationDominionEnum.VerbThresholds.ToString(), verb.Thresholds);
        situationCreationCommand.CommandQueue.Add(verbSlotsCommand);


        //get the ongoing slot specs for the recipe, and then add in any more specs implied by the existence of possible contents. I think this is
        //in case a recipe changed, but maybe it's also an alt/linked thing; maybe actually because of the null slotspecifications with transient verbs
        var recipeSlotSpecs = ImportSituationOngoingSlotSpecs(htSituationValues, recipe.Slots);
        var recipeSlotsCommand =
            new PopulateDominionCommand(SituationDominionEnum.RecipeThresholds.ToString(), recipeSlotSpecs);
        situationCreationCommand.CommandQueue.Add(recipeSlotsCommand);


        //ahhh... we need to do these explicitly because the situation has been created as AlreadyInState, meaning that it hasn't gone through its usual lifecycle.
        situationCreationCommand.CommandQueue.Add(new PopulateDominionCommand(SituationDominionEnum.Storage.ToString(), new SphereSpec(typeof(SituationStorageSphere), nameof(SituationStorageSphere))));
        situationCreationCommand.CommandQueue.Add(new PopulateDominionCommand(SituationDominionEnum.Output.ToString(), new SphereSpec(typeof(OutputSphere), nameof(OutputSphere))));



var tokenCreationCommand = new TokenCreationCommand(situationCreationCommand, tokenLocation);

        return tokenCreationCommand;
    }

    private List<SphereSpec> ImportSituationOngoingSlotSpecs(Hashtable htSituation, List<SphereSpec> ongoingSlotsForRecipe)
        {
            List<SphereSpec> ongoingSlotSpecs = new List<SphereSpec>();

            

            if (htSituation.ContainsKey(SaveConstants.SAVE_ONGOINGSLOTELEMENTS))
            {
                var htOngoingSlotStacks = htSituation.GetHashtable(SaveConstants.SAVE_ONGOINGSLOTELEMENTS);

                foreach (string slotPath in htOngoingSlotStacks.Keys)
                {
                    var slotId = slotPath.Split(FucinePath.SPHERE)[0];
                    var slotSpec = new SphereSpec(typeof(ThresholdSphere), slotId);
                    ongoingSlotSpecs.Add(slotSpec);
                }
            }

            else
            {
                //we don't have any elements in ongoing slots - but we might still have an empty slot from the recipe, which isn't tracked in the save
                //so add the slot to the spec anyway
               foreach (var slot in ongoingSlotsForRecipe)
                    ongoingSlotSpecs.Add(slot);
            }

            return ongoingSlotSpecs;
        }          

    private static StateEnum GetSituationState(Hashtable htSituationValues)
    {
        return (StateEnum) Enum.Parse(typeof(StateEnum),
            htSituationValues[SaveConstants.SAVE_SITUATIONSTATE].ToString());
    }

    private static Verb GetSituationVerb(Hashtable htSituationValues)
    {
        var verbId = htSituationValues[SaveConstants.SAVE_VERBID].ToString();
        var situationVerb = Watchman.Get<Compendium>().GetEntityById<Verb>(verbId);
        if (situationVerb == null)
            situationVerb = NullVerb.Create();
        return situationVerb;
    }

    private Recipe GetSituationRecipe(Hashtable htSituationValues, Verb situationVerb)
    {
        var recipeId = TryGetStringFromHashtable(htSituationValues, SaveConstants.SAVE_RECIPEID);
        var recipe = Watchman.Get<Compendium>().GetEntityById<Recipe>(recipeId);
        if (recipe == null)
            recipe = Recipe.CreateSpontaneousHintRecipe(situationVerb);
        return recipe;
    }

    private void AddElementStacksFromSituationValuesHashTableToDominionSphere(SituationCreationCommand situationCommand, Hashtable htSituationValues,string elementsWithThisHashtableKey, SituationDominionEnum toThisDominion)
    {

        if (htSituationValues.ContainsKey(elementsWithThisHashtableKey))
        {
            var htElements = htSituationValues.GetHashtable(elementsWithThisHashtableKey);

            foreach (var cmd in situationCommand.CommandQueue)
            {
                if (cmd is PopulateDominionCommand pdc)
                {
                    if (pdc.Identifier == toThisDominion.ToString())
                    {
                        var sphereCommandToPopulate = pdc.Spheres.FirstOrDefault();
                        if (sphereCommandToPopulate == null)
                            return;
                        else
                            AddElementsFromHashTableToSphereCommand(sphereCommandToPopulate, htElements);
                    }
                }
            }
        }
    }



    private void AddNotesToSituationCommand(SituationCreationCommand situationCommand, Hashtable htSituationValues,Verb verb)
    {
        if (htSituationValues.ContainsKey(SaveConstants.SAVE_SITUATIONNOTES))
        {
                var htSituationNotes = htSituationValues.GetHashtable(SaveConstants.SAVE_SITUATIONNOTES);

            foreach (var k in htSituationNotes.Keys)
            {
                var htThisOutput = htSituationNotes.GetHashtable(k);
                //NOTE: I called it 'title' in the note itself, cos it's the only text in the note.
                string title;
                if (htThisOutput[SaveConstants.SAVE_TITLE] != null)
                    title = htThisOutput[SaveConstants.SAVE_TITLE].ToString();
                else
                    title = "..."; //just catching possible empty descs so they don't blow up

                situationCommand.CommandQueue.Add(new NotifySituationCommand(verb.Label,title,Context.Unknown()));
            }

            
        }
        //{
        //    var notes = new SortedDictionary<int, Notification>();

            //    var htSituationNotes = htSituationValues.GetHashtable(SaveConstants.SAVE_SITUATIONNOTES);

            //    foreach (var k in htSituationNotes.Keys)
            //    {
            //        var htThisOutput = htSituationNotes.GetHashtable(k);
            //        //NOTE: distinct titles not currently used, but probably will be again
            //        string title;
            //        if (htThisOutput[SaveConstants.SAVE_TITLE] != null)
            //            title = htThisOutput[SaveConstants.SAVE_TITLE].ToString();
            //        else
            //            title = "..."; //just catching possible empty descs so they don't blow up


            //        var notificationForSituationNote = new Notification(title, title);

            //        if (int.TryParse(k.ToString(), out var order))
            //            notes.Add(order, notificationForSituationNote);
            //        else
            //            notes.Add(notes.Count, notificationForSituationNote);
            //    }
            //}
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

        var jsonString = ht[key].ToString();
        int returnValue;
        if (int.TryParse(jsonString, out returnValue))
            return returnValue;
        else
            return 0;
    }

    private float? TryGetNullableFloatFromHashtable(Hashtable ht, string key)
    {
        if (!ht.ContainsKey(key))
            return null;

        var jsonString = ht[key].ToString();
        float returnValue;
        if (float.TryParse(jsonString, out returnValue))
            return returnValue;
        else
            return null;
    }
}