using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using TabletopUi.Scripts.Services;

namespace SecretHistories.Infrastructure
{

    public enum StoreClient
    {
        Steam = 1,
        Gog = 2

    }

    public interface IStoreFrontClientProvider
    {
        void SetAchievement(string achievementId, bool setStatus);
        Storefront Storefront { get; }
    }


    


    

}
