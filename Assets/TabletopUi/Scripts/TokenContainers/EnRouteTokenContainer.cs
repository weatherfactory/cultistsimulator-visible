using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Enums;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Interfaces;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.TokenContainers
{
    public class EnRouteTokenContainer : TokenContainer, IDraggableHolder
    {
        public override ContainerCategory ContainerCategory { get; }

        public override string GetSaveLocationForToken(AbstractToken token)
        {
            throw new NotImplementedException();
        }

        public RectTransform RectTransform
        {
            get { return GetComponent<RectTransform>(); }
        }
    }
}
