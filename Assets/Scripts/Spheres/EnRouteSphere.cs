using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Fucine;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Interfaces;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.TokenContainers
{
    public class EnRouteSphere : Sphere
    {

        public override SphereCategory SphereCategory { get; }

        public TabletopSphere StartingContainer;


        public RectTransform RectTransform
        {
            get { return GetComponent<RectTransform>(); }
        }

        public void PrepareElementForSendAnim(Token token, TokenLocation destination) // "this reparents the card so it can animate properly" - okay, let's roll with that for now. But the line below is commented, so do we need it?
        {
            StartingContainer.AcceptToken(token, new Context(Context.ActionSource.DoubleClickSend)); // this reparents, sets container
            //stack.transform.position = ownerSituation.transform.position;
            token.Unshroud(true);
        }

        public void PrepareElementForGreedyAnim(Token stack, TokenLocation destination)
        {
            StartingContainer.AcceptToken(stack, new Context(Context.ActionSource.GreedySlot)); // "this reparents, sets container" - okay, let's roll with that for now
            stack.transform.position = destination.Position;
            stack.Unshroud(true);
        }

        public void MoveElementToSituationSlot(Token stack, TokenLocation destination, Sphere destinationSlot, float durationOverride = -1.0f)
        {
            var startPos = stack.GetComponent<RectTransform>().anchoredPosition3D;
            var endPos = destination.Position;

            float distance = Vector3.Distance(startPos, endPos);
            float duration = durationOverride > 0.0f ? durationOverride : Mathf.Max(0.3f, distance * 0.001f);

            var tokenAnimation = stack.gameObject.AddComponent<TokenAnimationToSlot>();
            tokenAnimation.onElementSlotAnimDone += ElementSendAnimDone;
            tokenAnimation.SetPositions(startPos, endPos);
            tokenAnimation.SetScaling(1f, 0.35f);
            tokenAnimation.SetDestination(destination, destinationSlot);

            destinationSlot.AddBlock(new ContainerBlock(BlockDirection.Inward,
                BlockReason.StackEnRouteToContainer));

            tokenAnimation.Begin(stack ,duration);
        }

        public void ElementSendAnimDone(Token element, TokenLocation destination,Sphere destinationSlot)
        {
            try
            {
                if (destinationSlot.Equals(null) || destinationSlot.Defunct)
                    element.ReturnToTabletop(new Context(Context.ActionSource.PlayerDrag));
                else
                    // Assign element to new slot
                    destinationSlot.AcceptToken(element, new Context(Context.ActionSource.AnimEnd));
                // Clear this whether the card arrived successfully or not, otherwise slot is locked for rest of session - CP
                destinationSlot.RemoveBlock(new ContainerBlock(BlockDirection.Inward,
                    BlockReason.StackEnRouteToContainer));
            }
            catch
            {
                // If anything goes wrong just dump the card back on the desk
                element.ReturnToTabletop(new Context(Context.ActionSource.PlayerDrag));
            }
        }

        public void ElementGreedyAnimDone(Token element, AnchorAndSlot anchorSlotPair)
        {
            if (anchorSlotPair.Threshold.Equals(null) || anchorSlotPair.Threshold.Defunct)
                return;

            anchorSlotPair.Threshold.AcceptToken(element, new Context(Context.ActionSource.AnimEnd));
            anchorSlotPair.Threshold.RemoveBlock(new ContainerBlock(BlockDirection.Inward,
                BlockReason.StackEnRouteToContainer));
        }


    }
}
