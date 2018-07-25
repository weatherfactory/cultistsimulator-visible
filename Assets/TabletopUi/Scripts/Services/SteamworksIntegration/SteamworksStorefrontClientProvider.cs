using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Noon;

namespace Assets.TabletopUi.Scripts.Infrastructure
{


    public class SteamworksStorefrontClientProvider : IStoreFrontClientProvider
    {




        public void SetAchievement(string achievementId, bool setStatus)
        {
   

            if (string.IsNullOrEmpty(achievementId))
                return;

        var steamClient = Facepunch.Steamworks.Client.Instance;
            if (steamClient == null)
            {
                                NoonUtility.Log("No Facepunch Steamworks client not initialised: not setting a Steam achievement for " + achievementId,10);
                return;
            }
            var achievement = steamClient.Achievements.Find(achievementId);
                if (achievement != null)
                {
                    if (setStatus && !achievement.State)
                    {
                        achievement.Trigger(true);
                        NoonUtility.Log("Set Steam achievement:" + achievementId, 1);
                    }

                    else if (!setStatus)
                        achievement.Reset();
                    else
                        NoonUtility.Log("Trying to set Steam achievement " + achievementId + ", but it's already set", 1);
                }
                else
                    NoonUtility.Log("Trying to set Steam achievement " + achievementId + ", but it doesn't exist", 11);
            

        }


    }




}
