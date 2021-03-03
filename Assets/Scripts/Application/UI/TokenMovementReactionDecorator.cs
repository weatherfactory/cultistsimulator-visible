using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.UI;
using SecretHistories.Constants.Events;
using SecretHistories.Enums;
using SecretHistories.Fucine;

using UnityEngine;

namespace SecretHistories.UI
{
    /// <summary>
    /// This allows reaction to a token being moved anywhere in the playfield - eg a 'come here, could interact' glow
    /// </summary>
    public class TokenMovementReactionDecorator : MonoBehaviour, ISphereCatalogueEventSubscriber
    { 
        
        private IInteractsWithTokens _decorated;

        public void Start()
        {
            _decorated = GetComponent<IInteractsWithTokens>();
            if(_decorated==null)
                NoonUtility.LogWarning("Can't initialise a TokenMovementReactionDecorator: IInteractswithTokens component missing on " + gameObject.name);
            else
                Watchman.Get<HornedAxe>().Subscribe(this);
        }

        public void NotifyTokensChanged(SphereContentsChangedEventArgs args)
        {
            //
        }

        public void OnTokenInteraction(TokenInteractionEventArgs args)
        {
            if (args.Interaction == Interaction.OnDragBegin || args.Interaction==Interaction.OnDrag) //both: circumstances may change as we move, after we've picked it up
            //and we may want to override WilLInteract glows
            {
                if (_decorated.CanInteractWithToken(args.Token))
                    _decorated.ShowPossibleInteractionWithToken(args.Token);

            }
            else if (args.Interaction == Interaction.OnDragEnd)
                _decorated.StopShowingPossibleInteractionWithToken(args.Token);


        }

    }
}
