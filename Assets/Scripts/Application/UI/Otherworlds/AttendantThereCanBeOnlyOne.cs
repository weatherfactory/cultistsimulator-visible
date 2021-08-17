using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.UI;
using SecretHistories.Constants.Events;
using SecretHistories.Enums;

namespace SecretHistories.Otherworlds
{
    /// <summary>
    /// when a token is unshrouded, purge all other tokens in this otherworld
    /// </summary>
   public class AttendantThereCanBeOnlyOne: AbstractOtherworldAttendant
    {
        
        public AttendantThereCanBeOnlyOne(Otherworld otherworld) : base(otherworld)
        {
        }

        public override void OnOtherworldEntryComplete()
        { }

        public override void OnOtherworldExitComplete()
        { }

        public override void OnTokensChangedForSphere(SphereContentsChangedEventArgs args)
        {
            if (args.Context.actionSource == Context.ActionSource.Unshroud)
            {
                var tokenToKeep = args.TokenChanged;
                foreach (var s in _otherworld.GetSpheres())
                {

                    foreach (var t in s.Tokens)
                    {
                        if(t!=tokenToKeep)
                            t.Retire(RetirementVFX.CardTransformWhite);
                    }
                }
            }
              
        }

        public override void OnTokenInteractionInSphere(TokenInteractionEventArgs args)
        {
          
        }
    }
}
