using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Noon;

namespace Assets.TabletopUi.Scripts.Infrastructure
{

    public interface IStoreClientProvider
    {
        StoreClientType GetCurrentStoreClientType();
        void SetAchievement(string id, bool setStatus);
    }


    public enum StoreClientType
    {
    None=0,
    Steam=1,
Gog=2

    }



    public class StoreClientProvider : IStoreClientProvider
    {

        public StoreClientType GetCurrentStoreClientType()
        {
            if (Facepunch.Steamworks.Client.Instance != null)
                return StoreClientType.Steam;
            else
                return StoreClientType.None;
        }

        public void SetAchievement(string id,bool setStatus)
        {
            if (!NoonUtility.AchievementsActive)
                return;

            if (string.IsNullOrEmpty(id))
                return;

            if (GetCurrentStoreClientType() == StoreClientType.Steam)
            { 
                var steamClient = Facepunch.Steamworks.Client.Instance;
                var achievement = steamClient.Achievements.Find(id);
                if (achievement == null)
                    return;

                if (setStatus && !achievement.State)
                    achievement.Trigger(true);
                else if (!setStatus)
                    achievement.Reset();
                else
                NoonUtility.Log("Trying to set achievement " + id + ", but it's already set",10);


            }

        }
    }
}
