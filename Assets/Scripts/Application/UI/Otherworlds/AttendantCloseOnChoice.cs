using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Constants.Events;
using SecretHistories.Spheres;
using SecretHistories.UI;

namespace SecretHistories.Assets.Scripts.Application.UI.Otherworlds
{
    /// <summary>
    /// When a token is placed in a given slot, moves that token to the ingress, and then closes the otherworld
    /// </summary>
   public class AttendantCloseOnChoice: AbstractOtherworldAttendant
    {
        private Sphere _respondToThisSphere;

        public override void OnOtherworldEntryComplete()
        { }

        public override void OnOtherworldExitComplete()
        { }

        public AttendantCloseOnChoice(Otherworld otherworld,Sphere sphere) : base(otherworld)
        {
            _respondToThisSphere = sphere;
        }

        public override void OnTokensChangedForSphere(SphereContentsChangedEventArgs args)
        {
             var tokenToTakeHome = args.TokenAdded;
            if (tokenToTakeHome == null)
                return;
            if(args.Sphere == _respondToThisSphere)
            {
               Watchman.Get<Numa>().HideCurrentOtherworld();
            }
        }
    }
}
