using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Elements.Manifestations
{
   public class MinimalManifestation:MonoBehaviour,IElementManifestation
    {
        public void DisplayVisuals(Element element)
        {
            //do nothing
        }

        public void UpdateText(Element element, int quantity)
        {
            //do nothing
        }

        public void ResetAnimations()
        {
            //do nothing
        }
    }
}
