using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Fucine;
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
    public class SimpleJSONGameDataImporter : IGameDataImporter
    {
        private  SpherePath windowSpherePath;
        private  SpherePath tabletopSpherePath;

        private Hashtable RetrieveHashedSaveFromFile(SourceForGameState source, bool temp = false)
        {
            var index = (int)source;

            string importJson = File.ReadAllText(
                temp ? NoonUtility.GetTemporaryGameSaveLocation(index) : NoonUtility.GetGameSaveLocation(index));
            Hashtable htSave = SimpleJsonImporter.Import(importJson);
            return htSave;
        }

        public bool IsSavedGameActive(SourceForGameState source, bool temp)
        {
            var htSave = RetrieveHashedSaveFromFile(source, temp);
            return htSave.ContainsKey(SaveConstants.SAVE_ELEMENTSTACKS) || htSave.ContainsKey(SaveConstants.SAVE_SITUATIONS);
        }

        private void OldFormatSave_TryRetrieveDefunctCharacter(Hashtable htSave, Character character)
        {
            var endingTriggeredForCharacterId =
                TryGetStringFromHashtable(htSave, SaveConstants.SAVE_CURRENTENDING);

            var endingTriggered = Registry.Get<Compendium>().GetEntityById<Ending>(endingTriggeredForCharacterId);



            character.Reset(null, endingTriggered);

        }


        public void ImportTableState(SourceForGameState source, TabletopSphere tabletop)
        {
            var htSave = RetrieveHashedSaveFromFile(source);

       windowSpherePath = new SpherePath(Registry.Get<Compendium>().GetSingleEntity<Dictum>().DefaultWindowSpherePath);
          tabletopSpherePath = new SpherePath(Registry.Get<Compendium>().GetSingleEntity<Dictum>().DefaultWorldSpherePath);


            var htElementStacks = htSave.GetHashtable(SaveConstants.SAVE_ELEMENTSTACKS);
            var htSituations = htSave.GetHashtable(SaveConstants.SAVE_SITUATIONS);

            if (tabletop != null)
            {
                ImportTabletopElementStacks(tabletop, htElementStacks);

                ImportSituations(tabletop, htSituations);
            }


        }

        public void ImportCharacter(SourceForGameState source, Character character)
        {

            var htSave = RetrieveHashedSaveFromFile(source);

            var htCharacter = htSave.GetHashtable(SaveConstants.SAVE_CHARACTER_DETAILS);
            if(htCharacter==null)
            {
                 OldFormatSave_TryRetrieveDefunctCharacter(htSave, character);
                 return;
            }
            

            var chosenLegacyForCharacterId =TryGetStringFromHashtable(htCharacter, SaveConstants.SAVE_ACTIVELEGACY);

            Legacy activeLegacy;
            Ending endingTriggered;


            if (string.IsNullOrEmpty(chosenLegacyForCharacterId))
            {
                activeLegacy = null;
            }
            else
            {
               activeLegacy = Registry.Get<Compendium>().GetEntityById<Legacy>(chosenLegacyForCharacterId);
            }

            var endingTriggeredForCharacterId =
                TryGetStringFromHashtable(htCharacter, SaveConstants.SAVE_CURRENTENDING);
            if (string.IsNullOrEmpty(endingTriggeredForCharacterId))
               endingTriggered = null;
            else
               endingTriggered = Registry.Get<Compendium>().GetEntityById<Ending>(endingTriggeredForCharacterId);

            

            character.Reset(activeLegacy,endingTriggered);


            if (htCharacter.ContainsKey(SaveConstants.SAVE_NAME))
                character.Name = htCharacter[SaveConstants.SAVE_NAME].ToString();


            if (htCharacter.ContainsKey(SaveConstants.SAVE_PROFESSION))
                character.Profession = htCharacter[SaveConstants.SAVE_PROFESSION].ToString();


            character.ClearExecutions();
            if (htCharacter.ContainsKey(SaveConstants.SAVE_EXECUTIONS))
            {
                var htExecutions = htCharacter.GetHashtable(SaveConstants.SAVE_EXECUTIONS);
                foreach(var key in htExecutions.Keys)
                    character.AddExecutionsToHistory(key.ToString(),GetIntFromHashtable(htExecutions,key.ToString()));
            }

            if (htCharacter.ContainsKey(SaveConstants.SAVE_PAST_LEVERS))
            {
                var htPastLevers = htCharacter.GetHashtable(SaveConstants.SAVE_PAST_LEVERS);
                foreach (var key in htPastLevers.Keys)
                {
                    //var enumKey = (LegacyEventRecordId) Enum.Parse(typeof(LegacyEventRecordId), key.ToString());
                    string value = htPastLevers[key].ToString();
                    if(!string.IsNullOrEmpty(value))
                        character.SetOrOverwritePastLegacyEventRecord(key.ToString().ToLower(), htPastLevers[key].ToString()); //hack: we used to have camel-cased enum values as keys and they may still exist in older saves

                }
            }

            if (htCharacter.ContainsKey(SaveConstants.SAVE_FUTURE_LEVERS))
            {
                var htFutureLevers = htCharacter.GetHashtable(SaveConstants.SAVE_FUTURE_LEVERS);
                foreach (var key in htFutureLevers.Keys)
                {
                  //  var enumKey = (LegacyEventRecordId)Enum.Parse(typeof(LegacyEventRecordId), key.ToString());
                    character.SetFutureLegacyEventRecord(key.ToString().ToLower(), htFutureLevers[key].ToString()); //hack: we used to have camel-cased enum values as keys  and they may still exist in older saves

                }
            }


            Registry.Get<Compendium>().SupplyLevers(character);

            var htDecks = htSave.GetHashtable(SaveConstants.SAVE_DECKS);

            ImportDecks(character, htDecks);


        }
        

        private void ImportTabletopElementStacks(TabletopSphere tabletop, Hashtable htElementStacks)
        {

            var elementStackSpecifications = PopulateElementStackSpecificationsList(htElementStacks);

	
            foreach (var ess in elementStackSpecifications)
            {
                tabletop.ProvisionStackFromCommand(ess);
            }

        }

        private void ImportDecks(Character character, Hashtable htDeckInstances)
        {
            foreach (var k in htDeckInstances.Keys)
            {
                var htEachDeck = htDeckInstances.GetHashtable(k);

                DeckSpec spec = Registry.Get<Compendium>().GetEntityById<DeckSpec>(k.ToString());

                if (spec == null)
                    NoonUtility.Log("no deckspec found for saved deckinstance " + k.ToString());
                else
                {
                    character.UpdateDeckInstanceFromSave(spec, htEachDeck);
                }
            }
        
        }

        private void ImportSituations(TabletopSphere tabletop, Hashtable htSituations)
        {



            foreach (var locationInfo in htSituations.Keys)
            {
                var htSituationValues =htSituations.GetHashtable(locationInfo);
                string verbId = htSituationValues[SaveConstants.SAVE_VERBID].ToString();

                IVerb situationVerb = Registry.Get<Compendium>().GetEntityById<BasicVerb>(verbId);


                string recipeId = TryGetStringFromHashtable(htSituationValues, SaveConstants.SAVE_RECIPEID);
                var recipe = Registry.Get<Compendium>().GetEntityById<Recipe>(recipeId);
                if (recipe == null)
                    recipe = NullRecipe.Create(situationVerb);

                

                //This caters for the otherwise troublesome situation where a completed situation (no recipe) has been based on a created verb (no verb obj).
                if (situationVerb == null && recipe==null)
                    situationVerb = new TransientVerb(verbId, "","");

                var situationState= (StateEnum)Enum.Parse(typeof(StateEnum), htSituationValues[SaveConstants.SAVE_SITUATIONSTATE].ToString());

                //if (situationState == StateEnum.Unstarted)
                //    situationState = StateEnum.ReadyToReset; //this state didn't exist in the old save format. We need to set ReadyToReset, or the situation window will remain in its primordial condition

                //if (situationState == StateEnum.Ongoing)
                //    situationState = StateEnum.ReadyToContinue;
                //commented above out because the Enter() method on the new state classes *should* take care of it


                var command = new SituationCreationCommand(situationVerb, recipe, situationState,null);
                command.TimeRemaining = TryGetNullableFloatFromHashtable(htSituationValues, SaveConstants.SAVE_TIMEREMAINING);

                command.OverrideTitle = TryGetStringFromHashtable(htSituationValues, SaveConstants.SAVE_TITLE);

                string simplifiedSituationPath;

                string[] simplifiedSituationPathParts = locationInfo.ToString().Split(SpherePath.SEPARATOR);
                if(simplifiedSituationPathParts.Length!=3)
                {
                    NoonUtility.LogWarning($"We can't parse a situation locationinfo: {locationInfo}. So we're just picking the beginning of it to use as the situation path.");
                    simplifiedSituationPath = simplifiedSituationPathParts[0];
                    command.AnchorLocation = new TokenLocation(0, 0, 0, tabletopSpherePath);
                }
                else
                {
                    simplifiedSituationPath = simplifiedSituationPathParts[2];
                    float.TryParse(simplifiedSituationPathParts[0], out float anchorPosX);
                    float.TryParse(simplifiedSituationPathParts[1], out float anchorPosY);
                    command.AnchorLocation = new TokenLocation(anchorPosX, anchorPosY, 0, tabletopSpherePath);


                }

                command.SituationPath = new SituationPath(simplifiedSituationPath);

                float? posx = TryGetNullableFloatFromHashtable(htSituationValues, SaveConstants.SAVE_SITUATION_WINDOW_X);
                float? posy = TryGetNullableFloatFromHashtable(htSituationValues, SaveConstants.SAVE_SITUATION_WINDOW_Y);
                float? posz = TryGetNullableFloatFromHashtable(htSituationValues, SaveConstants.SAVE_SITUATION_WINDOW_Z);

                

           


                if(posx!=null && posy!=null && posz!=null)
                {
                    var windowPosition=new Vector3((float) posx,(float)posy,(float)posz);
                    command.WindowLocation=new TokenLocation(windowPosition, windowSpherePath);
                }

                command.Open = htSituationValues[SaveConstants.SAVE_SITUATION_WINDOW_OPEN].MakeBool();


                if (htSituationValues.ContainsKey(SaveConstants.SAVE_ONGOINGSLOTELEMENTS))
                {
                    var htOngoingSlots = htSituationValues.GetHashtable(SaveConstants.SAVE_ONGOINGSLOTELEMENTS);
                    List<SlotSpecification> ongoingSlotSpecs=new List<SlotSpecification>();
                    foreach (string slotPath in htOngoingSlots.Keys)
                    {

                        //shit a fuck. We only store the slot label, not the required / forbidden elements of the specification!
                        var slotId = slotPath.Split(SpherePath.SEPARATOR)[0];
                        var slotSpec = new SlotSpecification(slotId);
                        ongoingSlotSpecs.Add(slotSpec);

                    }

                    command.OngoingSlots = ongoingSlotSpecs;

                }


                //old code for this stuff below:
                //SaveLocationInfo for slots are recorded with an appended Guid. Everything up until the last separator is the slotId
                //var slotId = ess.LocationInfo.Split(SaveConstants.SEPARATOR)[0];

                //int lastSeparatorPosition = ess.LocationInfo.LastIndexOf(SaveConstants.SEPARATOR);
                //    var slotId = ess.LocationInfo.Substring(0, lastSeparatorPosition); //if lastseparatorposition zero-indexed is 4, length before separator - 1-indexed - is also 4
                //    if (slotTypeKey == SaveConstants.SAVE_STARTINGSLOTELEMENTS)

                //    var slotToFill = situation.GetSlotBySaveLocationInfoPath(slotId, slotTypeKey);
                //if (slotToFill != null) //a little bit robust if a higher level element slot spec has changed between saves
                //    //if the game can't find a matching slot, it'll just leave it on the desktop
                //    slotToFill.AcceptToken(stackToPutInSlot, new Context(Context.ActionSource.Loading));

                //if this was an ongoing slot, we also need to tell the situation that the slot's filled, or it will grab another

                var situation = Registry.Get<SituationBuilder>().CreateSituation(command);
                
                ImportSlotContents(situation,htSituationValues,  SaveConstants.SAVE_STARTINGSLOTELEMENTS);
                ImportSlotContents(situation, htSituationValues,  SaveConstants.SAVE_ONGOINGSLOTELEMENTS);
                ImportSituationStoredElements(htSituationValues, situation);
                ImportOutputs(htSituationValues, situation, tabletop);

                //this should happen last, because adding those stacks above can overwrite notes
                ImportSituationNotes(htSituationValues, situation);

                situation.ExecuteHeartbeat(0f); //flushes everything through and updates

            }
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
                    var slotPath = new SpherePath(situation.Path, ess.LocationInfo.Split(SpherePath.SEPARATOR)[0]);

                    var slot = Registry.Get<SphereCatalogue>().GetContainerByPath(slotPath);

                    slot.ProvisionStackFromCommand(ess);

                }
            }
        }

        private void ImportOutputs(Hashtable htSituationValues, Situation situation, TabletopSphere tabletop)
        {
         var outputStacks=ImportOutputStacks(htSituationValues, tabletop);
            situation.AcceptTokens(SphereCategory.Output,outputStacks);

        }

        private List<Token> ImportOutputStacks(Hashtable htSituationValues, TabletopSphere tabletop)
        {
            List<Token> outputStacks = new List<Token>();

            if (htSituationValues.ContainsKey(SaveConstants.SAVE_SITUATIONOUTPUTSTACKS))
            {

                var htSituationOutputStacks = htSituationValues.GetHashtable(SaveConstants.SAVE_SITUATIONOUTPUTSTACKS);

                    
                    var stackSpecification = PopulateElementStackSpecificationsList(htSituationOutputStacks);
                    foreach (var ess in stackSpecification)
                    {
                        outputStacks.Add(tabletop.ProvisionStackFromCommand(ess));
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
                
                foreach(var n in notes)
                    situation.SendNotificationToSubscribers(n.Value);
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
                    var stackToStore=Registry.Get<Limbo>().ProvisionStackFromCommand(ess);

                    situation.AcceptToken(SphereCategory.SituationStorage, stackToStore,
                        new Context(Context.ActionSource.Loading));
                }


            }
        }




        private List<StackCreationCommand> PopulateElementStackSpecificationsList(Hashtable htStacks)
        {
            var stackCreationCommand = new List<StackCreationCommand>();
            foreach (var locationInfoKey in htStacks.Keys)
            {
                var htEachStack= htStacks.GetHashtable(locationInfoKey);
                

                string elementId = TryGetStringFromHashtable(htEachStack, SaveConstants.SAVE_ELEMENTID);
                int elementQuantity = GetIntFromHashtable(htEachStack, SaveConstants.SAVE_QUANTITY);
                int lifetimeRemaining = GetIntFromHashtable(htEachStack, SaveConstants.LIFETIME_REMAINING);
                bool markedForConsumption = htEachStack[SaveConstants.MARKED_FOR_CONSUMPTION].MakeBool();
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
                context.StackSource=Source.Existing();

                stackCreationCommand.Add(new StackCreationCommand(
                    elementId,
                    elementQuantity,
                    locationInfoKey.ToString(),
                    mutations,
                    illuminations,
                    lifetimeRemaining,
                    markedForConsumption,
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
