using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure.Events;
using Assets.TabletopUi.Scripts.Interfaces;
using Noon;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class TokenMovementReactionDecorator : MonoBehaviour, ISphereCatalogueEventSubscriber
    {
        [SerializeField] public IInteractsWithTokens decorated;

        public void Start()
        {
            decorated = GetComponent<IInteractsWithTokens>();
            if(decorated==null)
                NoonUtility.LogWarning("Can't initialise a TokenMovementReactionDecorator: IInteractswithTokens component missing on " + gameObject.name);
            else
                Registry.Get<SphereCatalogue>().Subscribe(this);
        }

        public void NotifyTokensChanged(TokenInteractionEventArgs args)
        {
            //
        }

        public void OnTokenInteraction(TokenInteractionEventArgs args)
        {
            if (args.Interaction == Interaction.OnDragBegin)
            {
                if (decorated.CanInteractWithToken(args.Token))
                    decorated.ShowPossibleInteractionWithToken(args.Token);
                decorated.ShowPossibleInteractionWithToken(args.Token);

            }
            else if (args.Interaction == Interaction.OnDragEnd)
                decorated.StopShowingPossibleReactionToToken(args.Token);


        }

    }
}
