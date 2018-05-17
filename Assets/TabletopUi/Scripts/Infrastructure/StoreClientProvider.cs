using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Noon;

namespace Assets.TabletopUi.Scripts.Infrastructure
{

    public interface IStoreClientProvider
    {
        bool IsAvailable(StoreClient storeClient);
        void SetAchievement(string achievementId, bool setStatus);
    }


    public enum StoreClient
    {
    Steam=1,
Gog=2

    }



    public class StoreClientProvider : IStoreClientProvider
    {
        private bool _initialised;

        public StoreClientProvider()
        {
//#if UNITY_STANDALONE_LINUX
//#else
//            Galaxy.Api.IAuthListener authListener = new GogAuthListener();

//            Galaxy.Api.GalaxyInstance.ListenerRegistrar().Register(Galaxy.Api.GalaxyTypeAwareListenerAuth.GetListenerType(), authListener);

//            _initialised = true;

//#endif
        }

        public bool IsAvailable(StoreClient storeClient)
        {
            if(!_initialised)
                throw new ApplicationException("Store client provider wasn't initialised");
          //  if (storeClient == StoreClient.Steam && Facepunch.Steamworks.Client.Instance != null)
            //    return true;
            return false;
//            if (storeClient == StoreClient.Gog)
//#if UNITY_STANDALONE_LINUX
//                return false;
//#else
//               if (GogGalaxyManager.Instance == null)
//                    return false;
//            if (Galaxy.Api.GalaxyInstance.User().SignedIn())
//                return true;

//#endif
//            return false;
        }

        public void SetAchievement(string achievementId,bool setStatus)
        {
            if (!NoonUtility.AchievementsActive)
                return;

            if (string.IsNullOrEmpty(achievementId))
                return;

            //if (IsAvailable(StoreClient.Steam))
            //{ 
            //    var steamClient = Facepunch.Steamworks.Client.Instance;
            //    var achievement = steamClient.Achievements.Find(achievementId);
            //    if (achievement != null)
            //    { 
            //    if (setStatus && !achievement.State)
            //        {
            //            achievement.Trigger(true);
            //            NoonUtility.Log("Set Steam achievement:" + achievementId, 5);
            //        }

            //        else if (!setStatus)
            //        achievement.Reset();
            //    else
            //    NoonUtility.Log("Trying to set Steam achievement " + achievementId + ", but it's already set",10);
            //    }
            //    else
            //        NoonUtility.Log("Trying to set Steam achievement " + achievementId + ", but it doesn't exist", 10);
            //}

#if UNITY_STANDALONE_LINUX
#else
            if (IsAvailable(StoreClient.Gog))
            {

                var gogStats = Galaxy.Api.GalaxyInstance.Stats();

                Galaxy.Api.IUserStatsAndAchievementsRetrieveListener statsRetrieveListener = new AchievementRequest(achievementId,setStatus,gogStats);
                Galaxy.Api.IStatsAndAchievementsStoreListener statsStoreListener = new GogStatsAndAchievementsStoreListener();
                Galaxy.Api.GalaxyInstance.ListenerRegistrar().Register(Galaxy.Api.GalaxyTypeAwareListenerUserStatsAndAchievementsRetrieve.GetListenerType(), statsRetrieveListener);
                Galaxy.Api.GalaxyInstance.ListenerRegistrar().Register(Galaxy.Api.GalaxyTypeAwareListenerStatsAndAchievementsStore.GetListenerType(), statsStoreListener);

                gogStats.RequestUserStatsAndAchievements(); //when the request completes, the callback will fire Execute on the AchievementRequest we attached above

            }
#endif
        }


    }


    

}
