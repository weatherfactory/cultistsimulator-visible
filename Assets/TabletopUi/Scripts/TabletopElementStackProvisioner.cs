using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Services;
using UnityEngine;

namespace Assets.TabletopUi.Scripts
{
    public class TabletopElementStackProvisioner: IElementStackProvisioner
    {
        private Transform transform;
        public TabletopElementStackProvisioner(Transform t)
        {
            transform = t;
        }

        public IElementStack ProvisionElementStack(string elementId, int quantity)
        {
            IElementStack stack = PrefabFactory.CreateLocally<ElementStack>(transform);
            stack.SetQuantity(quantity);

        }
    }
}
