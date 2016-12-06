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
            var htElementStacks = htSave.GetHashtable(GameSaveManager.SAVE_ELEMENTSTACKS);
            var htSituations = htSave.GetHashtable(GameSaveManager.SAVE_SITUATIONS);

            ImportTabletopElementStacks(tabletopContainer, htElementStacks);

            ImportSituations(tabletopContainer, htSituations);

        }

        private static void ImportTabletopElementStacks(TabletopContainer tabletopContainer, Hashtable htElementStacks)
        {
            foreach (var locationInfo in htElementStacks.Keys)
            {
                var elementStacks =
                    NoonUtility.HashtableToStringStringDictionary(htElementStacks.GetHashtable(locationInfo));

                int quantity;
                var couldParse = Int32.TryParse(elementStacks[GameSaveManager.SAVE_QUANTITY], out quantity);
                if (!couldParse)
                    throw new ArgumentException("Couldn't parse " + elementStacks[GameSaveManager.SAVE_QUANTITY] + " for " +
                                                elementStacks[GameSaveManager.SAVE_ELEMENTID] + " as a valid quantity.");

                tabletopContainer.GetElementStacksManager()
                    .IncreaseElement(elementStacks[GameSaveManager.SAVE_ELEMENTID], quantity, locationInfo.ToString());
            }
        }

        private void ImportSituations(TabletopContainer tabletopContainer, Hashtable htSituations)
        {
            foreach (var locationInfo in htSituations.Keys)
            {
                var htSituationValues =htSituations.GetHashtable(locationInfo);

                IVerb situationVerb = compendium.GetVerbById(htSituationValues[GameSaveManager.SAVE_VERBID].ToString());

                string recipeId = TryGetStringFromHashtable(htSituationValues, GameSaveManager.SAVE_RECIPEID);
                var recipe = compendium.GetRecipeById(recipeId);

                var command = new SituationCreationCommand(situationVerb, recipe);
                command.TimeRemaining = TryGetNullableFloatFromHashtable(htSituationValues, GameSaveManager.SAVE_TIMEREMAINING);
                command.State = TryGetNullableSituationState(htSituationValues, GameSaveManager.SAVE_SITUATIONSTATE);

               var situationAnchor= tabletopContainer.CreateSituation(command, locationInfo.ToString());

                ImportSlotContents(htSituationValues, situationAnchor,tabletopContainer, GameSaveManager.SAVE_STARTINGSLOTELEMENTS);
                ImportSlotContents(htSituationValues, situationAnchor, tabletopContainer, GameSaveManager.SAVE_ONGOINGSLOTELEMENTS);

                ImportSituationStoredElements(htSituationValues, situationAnchor);
            }
        }

        private void ImportSituationStoredElements(Hashtable htSituationValues, ISituationAnchor situationAnchor)
        {

            if (htSituationValues.ContainsKey(GameSaveManager.SAVE_SITUATIONSTOREDELEMENTS))
            {
                var htElements = htSituationValues.GetHashtable(GameSaveManager.SAVE_SITUATIONSTOREDELEMENTS);
                var elementQuantitySpecifications = PopulateElementQuantitySpecificationsList(htElements);
                foreach (var eqs in elementQuantitySpecifications)   
                    situationAnchor.ModifyStoredElementStack(eqs.ElementId,eqs.ElementQuantity);                    
            }
        }


        private void ImportSlotContents(Hashtable htSituationValues,
            ISituationAnchor situationAnchor, TabletopContainer tabletopContainer,string slotTypeKey)
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
                    var slotToFill = situationAnchor.GetSlotFromSituation(eqs.LocationInfo, slotTypeKey);
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
                    elementValues[GameSaveManager.SAVE_ELEMENTID],
                    GetQuantityFromElementHashtable(elementValues), locationInfo.ToString()));
            }
            return elementQuantitySpecifications;
        }

        private int GetQuantityFromElementHashtable(Dictionary<string, string> elementValues)
        {
            int quantity;
            var couldParse = Int32.TryParse(elementValues[GameSaveManager.SAVE_QUANTITY], out quantity);
            if (!couldParse)
                throw new ArgumentException("Couldn't parse " + elementValues[GameSaveManager.SAVE_QUANTITY] + " for " +
                                            elementValues[GameSaveManager.SAVE_ELEMENTID] + " as a valid quantity.");
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
