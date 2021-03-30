using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Logic;
using Assets.Scripts.Application.Infrastructure.Events;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Spheres;
using SecretHistories.Assets.Scripts.Application.UI;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Infrastructure;
using SecretHistories.Spheres;
using SecretHistories.Tokens.TokenPayloads;
using UnityEngine;

namespace SecretHistories.UI
{
    public class OtherworldLayer: MonoBehaviour
    {
        [SerializeField] private OtherworldAnimation _otherworldAnimation;

        [Space]
        [SerializeField] List<Otherworld> Otherworlds;


        private Ingress _activeIngress;
        private Portal _activePortal;
        private Otherworld _activeOtherworld;


        public void Awake()
        {
            var w=new Watchman();
            w.Register(this);

            foreach(var o in Otherworlds)
                o.RegisterDominions();
        }

        public void Open(RectTransform atRectTransform,Ingress ingress)
        {
            

            var otherworldToOpen = Otherworlds.SingleOrDefault(o => o.EntityId == ingress.GetOtherworldId());
            if(otherworldToOpen==null)
                NoonUtility.LogWarning("Can't find otherworld with id " + ingress.GetOtherworldId());
            else
                otherworldToOpen.Show(atRectTransform,ingress);

        }





        public void SetupMap(string portalId)
        {

 

            //         //get card position names
            //         //populate card positions 1,2,3 from decks with names of positions
            //         cards = new ElementStack[3];

            //         // Display face-up card next to door

            //         var character=Registry.Get<Character>();
            //         var dealer=new Dealer(character);

            //         string doorDeckId = activeDoor.GetDeckName(0);
            //         DeckInstance doorDeck =character.GetDeckInstanceById(doorDeckId);
            //         if (doorDeck==null)
            //             throw new ApplicationException("MapController couldn't find a mansus deck for the specified door with ID " + doorDeckId);

            //         string subLocationDeck1Id = activeDoor.GetDeckName(1);
            //         DeckInstance subLocationDeck1 = character.GetDeckInstanceById(subLocationDeck1Id);
            //         if (doorDeck == null)
            //             throw new ApplicationException("MapController couldn't find a mansus deck for location1 with ID " + subLocationDeck1Id);

            //         string subLocationDeck2Id = activeDoor.GetDeckName(2);
            //         DeckInstance subLocationDeck2 = character.GetDeckInstanceById(subLocationDeck2Id);
            //         if (doorDeck == null)
            //             throw new ApplicationException("MapController couldn't find a mansus deck for location2 with ID " + subLocationDeck2Id);

            //DrawWithMessage dwm0 = dealer.DealWithMessage(doorDeck);
            //         cards[0] = BuildCard(activeDoor.cardPositions[0].transform.position, dwm0.DrawnCard,activeDoor.portalType, dwm0.WithMessage);
            //         cards[0].Unshroud(true);

            //         // Display face down cards next to locations
            //int counter = 0;
            //         DrawWithMessage dwm1 = dealer.DealWithMessage(subLocationDeck1);
            //while (dwm1.DrawnCard == dwm0.DrawnCard)
            //{
            //	dwm1 = dealer.DealWithMessage(subLocationDeck1);	// Repeat until we get a non-matching card
            //	counter++;
            //	Debug.Assert( counter<10, "SetupMap() : Unlikely number of retries. Could be stuck in while loop?" );
            //}
            //         cards[1] = BuildCard(activeDoor.cardPositions[1].transform.position, dwm1.DrawnCard, activeDoor.portalType,  dwm1.WithMessage);
            //         cards[1].Shroud(true);

            //counter = 0;
            //         DrawWithMessage dwm2 = dealer.DealWithMessage(subLocationDeck2);
            //while (dwm2.DrawnCard == dwm1.DrawnCard || dwm2.DrawnCard == dwm0.DrawnCard)
            //{
            //	dwm2 = dealer.DealWithMessage(subLocationDeck2);	// Repeat until we get a non-matching card
            //	counter++;
            //	Debug.Assert( counter<10, "SetupMap() : Unlikely number of retries. Could be stuck in while loop?" );
            //}
            //         cards[2] = BuildCard(activeDoor.cardPositions[2].transform.position, dwm2.DrawnCard, activeDoor.portalType, dwm2.WithMessage);
            //         cards[2].Shroud(true);

            //         // When one face-down card is turned, remove all face up cards.
            //         // On droping on door: Return
        }


    }
}
