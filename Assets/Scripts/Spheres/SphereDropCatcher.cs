using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Spheres
{
    public class SphereDropCatcher: MonoBehaviour,IDropHandler
    {
        public Sphere Sphere;


        public void OnDrop(PointerEventData eventData)
        {
            if (Sphere.GoverningSlotSpecification.Greedy) // we're greedy? No interaction.
                return;

            var token = eventData.pointerDrag.GetComponent<Token>();

            if (token != null)
                Sphere.TryAcceptToken(token, new Context(Context.ActionSource.PlayerDrag));

        }
    }
}
