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
using SecretHistories.Constants.Events;
using SecretHistories.Spheres;
using UnityEngine;

namespace SecretHistories.NullObjects
{
   public class NullToken: Token
   {
       private static NullToken _instance;
       
       public override bool Retire(RetirementVFX rvfx)
       {
           Defunct = true;
           return true;
       }

       public override Sphere Sphere
       {
           get => Watchman.Get<HornedAxe>().GetDefaultSphere();

        set => throw new NotImplementedException();
       }


       public override void Manifest()
       {
           //
       }

       public override void Remanifest(RetirementVFX vfx)
       {
           //
       }

       public override bool IsValid()
       {
           return false;
       }

        public static NullToken Create()
        {
            if(_instance == null)
            {
                    var obj=new GameObject("NullToken");
                    var nullTokenComponent = obj.AddComponent<NullToken>();
                    nullTokenComponent.SetPayload(NullElementStack.Create());
                    _instance = nullTokenComponent;
            }

            return _instance;
         }
   }
}
