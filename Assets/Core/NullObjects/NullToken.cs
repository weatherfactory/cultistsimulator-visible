using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure.Events;
using UnityEngine;

namespace Assets.Core.NullObjects
{
   public class NullToken:Token
    {
        public override void Manifest()
        {
          //
        }

        public override void Remanifest(RetirementVFX vfx)
        {
        //
        }

        public override void ReactToDraggedToken(TokenInteractionEventArgs args)
        {
        //
        }

        public override void SituationStateChanged(Situation situation)
        {
          //
        }

        public override void OnElementStackStateChanged(ElementStack stack)
        {
           //
        }

        public static NullToken Create()
        {
           return new GameObject().AddComponent<NullToken>();

        }
    }
}
