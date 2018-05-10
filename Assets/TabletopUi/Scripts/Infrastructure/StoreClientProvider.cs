using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy.Api;
using Noon;

namespace Assets.TabletopUi.Scripts.Infrastructure
{

    public interface IStoreClientProvider
    {
        bool IsAvailable(StoreClient storeClient);
        void SetAchievement(string id, bool setStatus);
    }


    public enum StoreClient
    {
    Steam=1,
Gog=2

    }



    public class StoreClientProvider : IStoreClientProvider
    {

        public bool IsAvailable(StoreClient storeClient)
        {
            if (storeClient == StoreClient.Steam && Facepunch.Steamworks.Client.Instance != null)
                return true;
            if (storeClient == StoreClient.Gog)
                if (GogGalaxyManager.Instance == null)
                    return false;
            if (!GogGalaxyManager.IsInitialized())
                return false;
            if (!GalaxyInstance.User().SignedIn())
                return false;
            if (!GalaxyInstance.User().IsLoggedOn())
                return false;



            return true;
        }

        public void SetAchievement(string id,bool setStatus)
        {
            if (!NoonUtility.AchievementsActive)
                return;

            if (string.IsNullOrEmpty(id))
                return;

            if (IsAvailable(StoreClient.Steam))
            { 
                var steamClient = Facepunch.Steamworks.Client.Instance;
                var achievement = steamClient.Achievements.Find(id);
                if (achievement != null)
                { 
                if (setStatus && !achievement.State)
                    achievement.Trigger(true);
                else if (!setStatus)
                    achievement.Reset();
                else
                NoonUtility.Log("Trying to set Steam achievement " + id + ", but it's already set",10);
                }
                else
                    NoonUtility.Log("Trying to set Steam achievement " + id + ", but it doesn't exist", 10);
            }

            if (IsAvailable(StoreClient.Gog))
            {
                NoonUtility.Log("Logged on!",1);
            }

        }
    }
}
