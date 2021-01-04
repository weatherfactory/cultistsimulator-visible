using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Infrastructure;
using SecretHistories.Infrastructure.Events;
using SecretHistories.Interfaces;


namespace SecretHistories.Spheres.Angels
{
    public class TidyAngel: IAngel,ISphereEventSubscriber
    {
        public void Act(float interval)
        {

        }

        public void SetMinisterTo(Sphere sphere)
        {
          //
        }

        public void SetWatch(Sphere sphere)
        {
             sphere.Subscribe(this);
        }

        public void OnTokensChangedForSphere(TokenInteractionEventArgs args)
        {
           NoonUtility.Log("Token spotted: " + args.Token.name);
        }

        public void OnTokenInteractionInSphere(TokenInteractionEventArgs args)
        {
      //
        }
    }
}
