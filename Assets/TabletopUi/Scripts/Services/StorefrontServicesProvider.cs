using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.TabletopUi.Scripts.Infrastructure;
using Noon;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Services
{
   public class StorefrontServicesProvider
   {
       private IStoreFrontClientProvider _steamClientProvider;
       private IStoreFrontClientProvider _gogClientProvider;
        public void InitialiseForStorefrontClientType(StoreClient clientType)
        {
            if (clientType == StoreClient.Steam)
            {
                _steamClientProvider=new SteamworksStorefrontClientProvider();

            }
            if (clientType == StoreClient.Gog)
            {
#if UNITY_STANDALONE_LINUX
return;
#elif UNITY_STANDALONE_OSX
                return;
#else
        
                _gogClientProvider = new GOGStorefrontProvider();
               return;
#endif

            }

        }
        public void SetAchievementForCurrentStorefronts(string achievementId, bool setStatus)
        {
            if (!NoonUtility.AchievementsActive)
                return;
            if(_steamClientProvider!=null)
                _steamClientProvider.SetAchievement(achievementId,setStatus);

            if(_gogClientProvider!=null)
                _gogClientProvider.SetAchievement(achievementId,setStatus);

        }
    }
}
