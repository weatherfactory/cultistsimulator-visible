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

namespace Assets.TabletopUi.Scripts.TokenContainers
{
    public class WindowsSphere: Sphere
    {
        public override ContainerCategory ContainerCategory
        {
            get { return ContainerCategory.World; }
        }
    }
}
