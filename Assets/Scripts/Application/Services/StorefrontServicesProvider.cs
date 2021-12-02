using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SecretHistories.Constants;
using SecretHistories.Constants.Modding;

using UnityEngine;

namespace SecretHistories.Services
{
   public class StorefrontServicesProvider: MonoBehaviour
   {
       [SerializeField]
       private SteamManager _steamManager;
       [SerializeField]
       private GogGalaxyManager _gogGalaxyManager;
        
       private IStoreFrontClientProvider _steamClientProvider;
#pragma warning disable 649
       private IStoreFrontClientProvider _gogClientProvider; //it is assigned, it's just in a compile block below
#pragma warning restore 649
        public void InitialiseForStorefrontClientType(StoreClient clientType)
        {
            if (clientType == StoreClient.Steam)
            {
                _steamManager.TryInitialise();
                if (!SteamManager.Initialized)
                {
                    NoonUtility.Log("Trying to initialise Steam client provider, but SteamManager isn't initialised");
                    return;

                }

                NoonUtility.Log("Initialising Steam client provider");
                _steamClientProvider =new SteamworksStorefrontClientProvider();

            }


            //we're integrating with GOG again
            if (clientType == StoreClient.Gog )
            {
                _gogGalaxyManager.TryInitialise();

                if (!GogGalaxyManager.IsInitialized())
                {
                    NoonUtility.LogWarning("Trying to initialise GOG client provider, but GogGalaxyManager hasn't been successfully initialised");
                    return;

                }

                NoonUtility.Log("Initialising GOG client provider");
                _gogClientProvider = new GOGStorefrontProvider();
                return;
            }

        }

        public bool IsAvailable(StoreClient clientType)
        {
            if (clientType == StoreClient.Steam)
                return _steamClientProvider != null;

            else if (clientType == StoreClient.Gog)
                return _gogClientProvider != null;

            else
                return false;


        }

        public void SetAchievementForCurrentStorefronts(string achievementId, bool setStatus)
        {
            try
            {
            
                if (!NoonUtility.AchievementsActive)
                    return;
                if(_steamClientProvider!=null)
                    _steamClientProvider.SetAchievement(achievementId,setStatus);

                if(_gogClientProvider!=null)
                    _gogClientProvider.SetAchievement(achievementId,setStatus);
            }
            catch (Exception e)
            {
                //let's try not to bring down the house because the phone line isn't working
                NoonUtility.Log("WARNING: tried to set achievement" + achievementId + ", but failed: " + e.Message);
                //throw;
            }
            
        }

        public List<SubscribedStorefrontMod> GetSubscribedItems()
        {
            
                if (_steamClientProvider is SteamworksStorefrontClientProvider steamClient)
                   return steamClient.GetSubscribedItems();
                else
                    return new List<SubscribedStorefrontMod>();
                
        }

#pragma warning disable 1998
        public async Task UploadModForCurrentStorefront(Mod modToUpload)

        { if(_steamClientProvider is SteamworksStorefrontClientProvider steamClient)
        
                steamClient.UploadMod(modToUpload);
            //    await Task.Delay(3000); this just checks it's actually working asynchronously. I probably don't actually need async cos Steam, anyway!
        }

        public async Task UpdateModForCurrentStorefront(Mod modToUpload,string publishedFileId)
        {
            if (_steamClientProvider is SteamworksStorefrontClientProvider steamClient)
                steamClient.UpdateMod(modToUpload,publishedFileId);
        }

#pragma warning restore 1998
    }
}
