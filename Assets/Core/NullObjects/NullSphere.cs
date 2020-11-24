using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Commands;
using Assets.Core.Enums;
using Assets.Core.Fucine;
using Assets.Core.NullObjects;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;
using Noon;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.TokenContainers
{
    public class NullSphere:Sphere
    {
        public override SphereCategory SphereCategory => SphereCategory.Null;
        public override bool PersistBetweenScenes => true;
        public override bool EnforceUniqueStacksInThisContainer =>false;
        public override bool ContentsHidden => true;

        public override bool CurrentlyBlockedFor(BlockDirection direction)
        {
            if (direction == BlockDirection.Outward)
                return false; //might legitimately want to remove something from the null container

            return true;
        }

        public override SpherePath GetPath()
        {
            return new SpherePath("-");
        }

        public override Token ProvisionStackFromCommand(StackCreationCommand stackCreationCommand)
        {
            NoonUtility.LogWarning("Trying to provision a stack in a null sphere, which is possibly a mistake: " + stackCreationCommand.ElementId);

            return NullToken.Create();
        }


    }
}
