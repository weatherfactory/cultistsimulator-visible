using Noon;
using Steamworks;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public class SteamworksStorefrontClientProvider : IStoreFrontClientProvider
    {
        private readonly CGameID _gameId;

        public SteamworksStorefrontClientProvider()
        {
            if (!SteamManager.Initialized)
                return;

            // Cache the GameID for use in the Callbacks
            _gameId = new CGameID(SteamUtils.GetAppID());
            
            // Set up Steam callbacks
            Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
            
            // Fetch the initial list of achievements
            SteamUserStats.RequestCurrentStats();
        }

        public void SetAchievement(string achievementId, bool setStatus)
        {
            if (string.IsNullOrEmpty(achievementId))
                return;

            if (!SteamManager.Initialized)
            {
                NoonUtility.Log($"No Steamworks client initialised: not setting a Steam achievement for {achievementId}");
                return;
            }

            if (!SteamUserStats.GetAchievement(achievementId, out var state))
            {
                NoonUtility.Log($"Trying to set Steam achievement {achievementId}, but it doesn't exist");
                return;
            }
            if (state != setStatus)
            {
                if (setStatus)
                    SteamUserStats.SetAchievement(achievementId);
                else
                    SteamUserStats.ClearAchievement(achievementId);
                NoonUtility.Log((setStatus ? "Set" : "Unset") + $" Steam achievement: {achievementId}");
            }
            else
                NoonUtility.Log(
                    "Trying to " + (setStatus ? "set" : "unset") 
                                 + $" Steam achievement {achievementId}, but it's already " 
                                 + (state ? "set" : "unset"), 
                    1);
        }
        
        private void OnUserStatsReceived(UserStatsReceived_t pCallback) 
        {
            if (!SteamManager.Initialized)
                return;

            // Ignore callbacks from other games
            if ((ulong) _gameId != pCallback.m_nGameID) 
                return;

            NoonUtility.Log(pCallback.m_eResult == EResult.k_EResultOK
                ? "Received achievements from Steam"
                : $"Failed to fetch achievements: Code {pCallback.m_eResult}");
        }
    }
}
