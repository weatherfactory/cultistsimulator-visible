using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.TabletopUi.Scripts.Infrastructure;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Services
{
   public class StorefrontServicesProvider
    {
        public void InitialiseStorefrontClient(StoreClient client)
        {
            if (client == StoreClient.Steam)
                return;
            if (client == StoreClient.Gog)
            {
                if(SystemInfo.operatingSystemFamily==OperatingSystemFamily.Linux)
                    throw new ApplicationException("Can't initialise GOG services on Linux");
                else
                return;
            }
        }
        public void SetAchievementForCurrentStorefronts(string achievementId, bool setStatus)
        {

        }
    }
}
