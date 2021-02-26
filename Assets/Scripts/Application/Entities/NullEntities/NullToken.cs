using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Elements;
using SecretHistories.Elements.Manifestations;
using SecretHistories.Constants.Events;
using SecretHistories.Spheres;
using UnityEngine;

namespace SecretHistories.NullObjects
{
   public class NullToken: Token
   {
       
 
       public bool Defunct { get; private set; }
       public Sphere Sphere { get; }

       public bool Retire(RetirementVFX rvfx)
       {
           Defunct = true;
           return true;
       }


        public static NullToken Create()
        {
            var obj=new GameObject("NullToken");
            var nullTokenComponent = obj.AddComponent<NullToken>();
            nullTokenComponent.SetPayload(NullElementStack.Create());
            return nullTokenComponent;
        }
   }
}
