using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Fucine;
using SecretHistories.UI;
using UnityEngine;
using SecretHistories.Services;
using SecretHistories.Entities;
using SecretHistories.Enums;
using Assets.Logic;

namespace SecretHistories.Constants
{
    public class MapController: MonoBehaviour
    {
 


      


        ElementStack BuildCard(Vector3 position, string id,PortalEffect portalType,string mansusJournalEntryMessage)
        {
        //    var newCard =
        //        _mapSphere.ProvisionElementStackToken(id, 1, Source.Fresh(),
        //            new Context(Context.ActionSource.Loading)) as ElementStack;
            

        //    newCard.IlluminateLibrarian.AddMansusJournalEntry(mansusJournalEntryMessage);

        //    // Accepting stack will trigger overlap checks, so make sure we're not in the default pos but where we want to be.
        //    newCard.transform.position = position;
            
        //    // Accepting stack may put it to pos Vector3.zero, so this is last
        //    newCard.transform.position = position;
        //    newCard.transform.localScale = Vector3.one;
        //    newCard.onTurnFaceUp += HandleOnCardTurned;
        //    return newCard;
        //}

        //void HandleOnCardTurned(ElementStack cardTurned) {
        //    if (cards != null)
        //        for (int i = 0; i < cards.Length; i++)
        //            if (cards[i] != cardTurned)
        //            {
        //                cards[i].Unshroud();
        //                cards[i].Retire(RetirementVFX.CardLightDramatic);
        //            }

        //    cards = null;
        return null;
        }

        void HandleOnSlotFilled(ElementStack stack) {
          //  var activeDoor = _mapSphere.GetActiveDoor();
            //HideMansusMap(activeDoor.transform, stack);
        }

        public void CleanupMap(ElementStack pickedStack) {
            //var activeDoor = _mapSphere.GetActiveDoor();
            //activeDoor.onCardDropped -= HandleOnSlotFilled;
            //_mapSphere.SetActiveDoor(PortalEffect.None);

            //// Kill all still existing cards
            //if (cards != null) {
            //    foreach (var item in cards) {
            //        if (item != pickedStack)
            //            item.Retire(RetirementVFX.None);
            //    }
            //}

            //cards = null;
        }

        // -- ANIMATION ------------------------



    }
}
