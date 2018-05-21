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
        public void InitialiseForStorefrontClientType(StoreClient clientType)
        {
            if (clientType == StoreClient.Steam)
            {
                _steamClientProvider=new SteamworksStorefrontClientProvider();

            }
                
            if (clientType == StoreClient.Gog)
            {
                if(SystemInfo.operatingSystemFamily==OperatingSystemFamily.Linux)
                    throw new ApplicationException("Can't initialise GOG services on Linux");
                else
                return;
            }
        }
        public void SetAchievementForCurrentStorefronts(string achievementId, bool setStatus)
        {
            if (!NoonUtility.AchievementsActive)
                return;
            if(_steamClientProvider!=null)
                _steamClientProvider.SetAchievement(achievementId,setStatus);

        }
    }
}
