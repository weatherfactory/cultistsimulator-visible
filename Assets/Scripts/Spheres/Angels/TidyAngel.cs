using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Infrastructure.Events;
using Assets.TabletopUi.Scripts.Interfaces;
using Noon;

namespace Assets.Scripts.Spheres.Angels
{
    public class TidyAngel: IAngel,ISphereEventSubscriber
    {

        public void MinisterTo(Sphere thresholdSphereToGrabTo, float interval)
        {
          //
        }

        public void WatchOver(Sphere sphere)
        {
             sphere.Subscribe(this);
        }

        public void NotifyTokensChangedForSphere(TokenInteractionEventArgs args)
        {
           NoonUtility.Log("Token spotted: " + args.Token.name);
        }

        public void OnTokenInteractionInSphere(TokenInteractionEventArgs args)
        {
      //
        }
    }
}
