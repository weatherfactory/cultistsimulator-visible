using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using SecretHistories.Elements;
using SecretHistories.Elements.Manifestations;
using SecretHistories.Infrastructure.Events;
using SecretHistories.Interfaces;
using SecretHistories.TokenContainers;
using UnityEngine;

namespace SecretHistories.NullObjects
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
            gameObject.name = nameof(NullToken);

       }

       public override void Populate(ElementStack elementStack)
       {
           gameObject.name = "NullTokenForUnpopulatedElementStack";
       }

       public override bool Retire(RetirementVFX rvfx)
       {
           this.Defunct = true;
           Destroy(gameObject);
           return true;
       }
        public override void Manifest()
        {
          //
        }

        public override void Remanifest(RetirementVFX vfx)
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
