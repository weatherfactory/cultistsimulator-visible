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
                                NoonUtility.Log("Trying to use an uninitialised Facepuch Steamworks client");
                return;
            }
            var achievement = steamClient.Achievements.Find(achievementId);
                if (achievement != null)
                {
                    if (setStatus && !achievement.State)
                    {
                        achievement.Trigger(true);
                        NoonUtility.Log("Set Steam achievement:" + achievementId, 5);
                    }

                    else if (!setStatus)
                        achievement.Reset();
                    else
                        NoonUtility.Log("Trying to set Steam achievement " + achievementId + ", but it's already set", 10);
                }
                else
                    NoonUtility.Log("Trying to set Steam achievement " + achievementId + ", but it doesn't exist", 10);
            

        }


    }




}
