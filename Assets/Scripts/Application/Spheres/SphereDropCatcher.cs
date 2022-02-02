using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.UI;
using SecretHistories.States.TokenStates;
using SecretHistories.Constants;
using SecretHistories.Enums;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SecretHistories.Spheres
{
    public class SphereDropCatcher: MonoBehaviour,IDropHandler,IPointerClickHandler
    {
        public Sphere Sphere;


        public void OnDrop(PointerEventData eventData)
        {
            if (Sphere.GoverningSphereSpec.Greedy) // we're greedy? No interaction.
                return;

            if (Sphere.CurrentlyBlockedFor(BlockDirection.Inward))
                return;

            var token = eventData.pointerDrag.GetComponent<Token>();

            if (token != null)
            {
                if (token.RequestingNoDirectDrop()) //This may now be redundant if we're using right-clicking
                    token.TryFulfilGhostPromise(new Context(Context.ActionSource.PlayerDrag));
                else    
                if (Sphere.TryAcceptToken(token, new Context(Context.ActionSource.PlayerDrag)))
                    token.Stabilise();
            }
              

        }


        public bool TryShowPredictedInteractionIfDropped(Token forToken)
        {
            return Sphere.TryDisplayGhost(forToken);
            
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(Sphere.Traversable) //move someones with a right-click
            {

                if (eventData.button == PointerEventData.InputButton.Right)
                {
                    var currentlyOpenSituation = Watchman.Get<Meniscate>().GetCurrentlyOpenSituation();
                    if(currentlyOpenSituation.Verb.Category==VerbCategory.Someone)
                    {
                        var token = currentlyOpenSituation.Token;
                        Sphere.TryDisplayGhost(token, eventData.pointerCurrentRaycast.worldPosition);
                        token.TryFulfilGhostPromise(Context.Unknown());
                    }
                }

            }
        }
    }
}
