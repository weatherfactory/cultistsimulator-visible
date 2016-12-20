using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Commands;
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
        void ImportSavedGameToContainer(TabletopContainer tabletopContainer, Hashtable htSave);        
    }

    public class GameDataImporter : IGameDataImporter
    {
        private ICompendium compendium;

        
        public GameDataImporter(ICompendium compendium)
        {
            this.compendium = compendium;
        }



        public void ImportSavedGameToContainer(TabletopContainer tabletopContainer, Hashtable htSave)
        {
            var htElementStacks = htSave.GetHashtable(SaveConstants.SAVE_ELEMENTSTACKS);
            var htSituations = htSave.GetHashtable(SaveConstants.SAVE_SITUATIONS);

            ImportTabletopElementStacks(tabletopContainer, htElementStacks);

            ImportSituations(tabletopContainer, htSituations);

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
                    .IncreaseElement(dictionaryElementStacks[SaveConstants.SAVE_ELEMENTID], quantity, locationInfo.ToString());
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

                var command = new SituationCreationCommand(situationVerb, recipe);
                command.TimeRemaining = TryGetNullableFloatFromHashtable(htSituationValues, SaveConstants.SAVE_TIMEREMAINING);
                command.State = TryGetNullableSituationState(htSituationValues, SaveConstants.SAVE_SITUATIONSTATE);

                var situationAnchor= tabletopContainer.CreateSituation(command, locationInfo.ToString());
                var situationController = situationAnchor.SituationController;

                ImportSlotContents(htSituationValues, situationController, tabletopContainer, SaveConstants.SAVE_STARTINGSLOTELEMENTS);
                ImportSlotContents(htSituationValues, situationController, tabletopContainer, SaveConstants.SAVE_ONGOINGSLOTELEMENTS);

                ImportSituationStoredElements(htSituationValues, situationController);

                ImportOutputNotes(htSituationValues, situationController, tabletopContainer);
            }
        }

        private void ImportOutputNotes(Hashtable htSituationValues, SituationController controller,TabletopContainer container)
        {
            if (htSituationValues.ContainsKey(SaveConstants.SAVE_SITUATIONOUTPUTS))
            {
                var htSituationOutputs = htSituationValues.GetHashtable(SaveConstants.SAVE_SITUATIONOUTPUTS);
                foreach (var k in htSituationOutputs.Keys)
                {
                    var htThisOutput = htSituationOutputs.GetHashtable(k);
                    var notificationForOutputNote=new Notification(htThisOutput[SaveConstants.SAVE_TITLE].ToString(),htThisOutput[SaveConstants.SAVE_DESCRIPTION].ToString());
                    var htOutputElements = htThisOutput.GetHashtable(SaveConstants.SAVE_OUTPUTELEMENTS);
                    var elementQuantitySpecifications = PopulateElementQuantitySpecificationsList(htOutputElements);
                    List<IElementStack> stacksForOutputNote=new List<IElementStack>();
                    foreach (var eqs in elementQuantitySpecifications)
                    {
                        stacksForOutputNote.Add(container.GetTokenTransformWrapper().ProvisionElementStack(eqs.ElementId,eqs.ElementQuantity));
                    }
                    controller.AddOutput(stacksForOutputNote,notificationForOutputNote);

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
                            .ProvisionElementStack(eqs.ElementId, eqs.ElementQuantity);
                    var slotToFill = controller.GetSlotBySaveLocationInfoPath(eqs.LocationInfo, slotTypeKey);
                    if (slotToFill != null) //a little bit robust if a higher level element slot spec has changed between saves
                        //if the game can't find a matching slot, it'll just leave it on the desktop
                        slotToFill.AcceptStack(stackToPutInSlot);

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

        private SituationState? TryGetNullableSituationState(Hashtable ht, string key)
        {
            if (!ht.ContainsKey(key))
                return null;

            return (SituationState)Enum.Parse(typeof(SituationState), ht[key].ToString());
        }

    }
    
}
