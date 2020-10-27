using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Enums;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.TokenContainers
{
    public class NullContainer:TokenContainer
    {
        public override ContainerCategory ContainerCategory => ContainerCategory.Null;

        public override string GetSaveLocationForToken(AbstractToken token)
        {
            return string.Empty;
        }

        public NullContainer Create()
        {
           var gameObject=new GameObject("NullContainer");
           return gameObject.AddComponent<NullContainer>();
           
        }
    }
}
