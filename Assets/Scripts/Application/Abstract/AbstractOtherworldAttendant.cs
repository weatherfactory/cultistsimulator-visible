using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Assets.Scripts.Application.UI;
using SecretHistories.Constants.Events;
using SecretHistories.Fucine;

namespace SecretHistories.Abstract
{
    public abstract class AbstractOtherworldAttendant: ISphereEventSubscriber
    {
        protected readonly Otherworld _otherworld;

        public AbstractOtherworldAttendant(Otherworld otherworld)
        {
            _otherworld = otherworld;
        }

        public virtual void OnTokensChangedForSphere(SphereContentsChangedEventArgs args)
        {
            
        }

        public virtual void OnTokenInteractionInSphere(TokenInteractionEventArgs args)
        {
            
        }
    }
}
