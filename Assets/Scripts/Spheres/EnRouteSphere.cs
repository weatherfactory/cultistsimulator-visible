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

        public override SphereCategory SphereCategory => SphereCategory.World;



        public void ElementGreedyAnimDone(Token element, AnchorAndSlot anchorSlotPair)
        {
            if (anchorSlotPair.Threshold.Equals(null) || anchorSlotPair.Threshold.Defunct)
                return;

            anchorSlotPair.Threshold.AcceptToken(element, new Context(Context.ActionSource.TravelArrived));
            anchorSlotPair.Threshold.RemoveBlock(new ContainerBlock(BlockDirection.Inward,
                BlockReason.InboundTravellingStack));
        }


    }
}
