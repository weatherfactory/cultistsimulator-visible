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
            IAuthListener authListener = new GogAuthListener();
            
            GalaxyInstance.ListenerRegistrar().Register(GalaxyTypeAwareListenerAuth.GetListenerType(), authListener);

            _initialised = true;
        }

        public bool IsAvailable(StoreClient storeClient)
        {
            if(!_initialised)
                throw new ApplicationException("Store client provider wasn't initialised");
            if (storeClient == StoreClient.Steam && Facepunch.Steamworks.Client.Instance != null)
                return true;
            if (storeClient == StoreClient.Gog)

            if (GogGalaxyManager.Instance == null)
                    return false;
            if (GalaxyInstance.User().SignedIn())
                return true;

            return false;
        }

        public void SetAchievement(string achievementId,bool setStatus)
        {
            if (!NoonUtility.AchievementsActive)
                return;

            if (string.IsNullOrEmpty(achievementId))
                return;

            if (IsAvailable(StoreClient.Steam))
            { 
                var steamClient = Facepunch.Steamworks.Client.Instance;
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
                NoonUtility.Log("Trying to set Steam achievement " + achievementId + ", but it's already set",10);
                }
                else
                    NoonUtility.Log("Trying to set Steam achievement " + achievementId + ", but it doesn't exist", 10);
            }

            if (IsAvailable(StoreClient.Gog))
            {
                var gogStats = GalaxyInstance.Stats();

                IUserStatsAndAchievementsRetrieveListener statsRetrieveListener = new AchievementRequest(achievementId,setStatus,gogStats);
                IStatsAndAchievementsStoreListener statsStoreListener = new GogStatsAndAchievementsStoreListener();
                GalaxyInstance.ListenerRegistrar().Register(GalaxyTypeAwareListenerUserStatsAndAchievementsRetrieve.GetListenerType(), statsRetrieveListener);
                GalaxyInstance.ListenerRegistrar().Register(GalaxyTypeAwareListenerStatsAndAchievementsStore.GetListenerType(), statsStoreListener);

                gogStats.RequestUserStatsAndAchievements(); //when the request completes, the callback will fire Execute on the AchievementRequest we attached above

            }

        }


    }


    public class GogAuthListener : IAuthListener
    {
        public override void OnAuthSuccess()
        {
            NoonUtility.Log("GOG Galaxy logged on?" + GalaxyInstance.User().IsLoggedOn(),10);
            NoonUtility.Log("GOG Galaxy signed in? " + GalaxyInstance.User().SignedIn(),10);
        }

        public override void OnAuthFailure(FailureReason failureReason)
        {
            NoonUtility.Log("GOG Galaxy auth failed:" + failureReason,10);
        }

        public override void OnAuthLost()
        {
            NoonUtility.Log("Authentication lost");
        }
    }

    public class AchievementRequest : IUserStatsAndAchievementsRetrieveListener

    {
        private string _forAchievementId;
        private bool _setStatus;
        private IStats _gogStats;

        public AchievementRequest(string achievementId, bool setStatus,IStats gogStats)
        {
            if (string.IsNullOrEmpty(achievementId))
                throw new ApplicationException("Whoa there, don't try to pass an empty achievementid to a GOG achievement request");
            if(gogStats==null)
                throw new ApplicationException("Whoa there, we need an actual stats object in our GOG achievement request");

            _forAchievementId = achievementId;
            _setStatus = setStatus;
            _gogStats = gogStats;
        }

        private void Execute()
        {
            if(_setStatus)
                _gogStats.SetAchievement(_forAchievementId);
            else
            _gogStats.ClearAchievement(_forAchievementId);
            _gogStats.StoreStatsAndAchievements();

            NoonUtility.Log("Set GOG achievement: " + _forAchievementId, 10);
        }

        public override void OnUserStatsAndAchievementsRetrieveSuccess(GalaxyID userID)
        {
            NoonUtility.Log("Retrieved achievements: about to set " + _forAchievementId + " as " + _setStatus,10);
            Execute();
        }

        public override void OnUserStatsAndAchievementsRetrieveFailure(GalaxyID userID, FailureReason failureReason)
        {
            NoonUtility.Log("Couldn't retrieve achievements: " + failureReason, 10);
        }

    }

    //not in use!
    public class AchievementList: IUserStatsAndAchievementsRetrieveListener

    {
        private IStats _gogStats;


        public AchievementList(IStats gogStats)
        {
            _gogStats = gogStats;
        }


        public override void OnUserStatsAndAchievementsRetrieveSuccess(GalaxyID userID)
        {
            NoonUtility.Log("Retrieved achievements: about to list them."); //or I would if there was something in the API for this
            
        }

        public override void OnUserStatsAndAchievementsRetrieveFailure(GalaxyID userID, FailureReason failureReason)
        {
            NoonUtility.Log("Couldn't retrieve achievements: " + failureReason, 10);
        }

    }

    public class GogStatsAndAchievementsStoreListener : IStatsAndAchievementsStoreListener
    {
        public override void OnUserStatsAndAchievementsStoreSuccess()
        {
            NoonUtility.Log("Stored achievements", 10);

        }

        public override void OnUserStatsAndAchievementsStoreFailure(FailureReason failureReason)
        {
            NoonUtility.Log("Couldn't store achievements: " + failureReason, 10);
        }
    }
    

}
