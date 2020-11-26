using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Elements;
using Assets.TabletopUi.Scripts.Elements.Manifestations;
using Assets.TabletopUi.Scripts.Infrastructure.Events;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.TokenContainers;
using UnityEngine;

namespace Assets.Core.NullObjects
{
   public class NullToken:Token
   {
       public NullManifestation NullManifestation;
       public NullElementStack NullElementStack;

       public override IVerb Verb => NullVerb.Create();

       public override void Awake()
       {
           TokenRectTransform = GetComponent<RectTransform>();
           canvasGroup = GetComponent<CanvasGroup>();
            _manifestation = NullManifestation;
            ElementStack = NullElementStack;

       }

       public override void Manifest(Type manifestationType)
       {
            //
       }


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

        

    }
}
