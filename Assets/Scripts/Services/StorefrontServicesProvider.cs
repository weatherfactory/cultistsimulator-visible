using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
using Noon;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Services
{
   public class StorefrontServicesProvider
   {
       private IStoreFrontClientProvider _steamClientProvider;
#pragma warning disable 649
       private IStoreFrontClientProvider _gogClientProvider; //it is assigned, it's just in a compile block below
#pragma warning restore 649
        public void InitialiseForStorefrontClientType(StoreClient clientType)
        {
            if (clientType == StoreClient.Steam && SteamManager.Initialized)
            {
                _steamClientProvider=new SteamworksStorefrontClientProvider();

            }

#if UNITY_STANDALONE_LINUX
return;
#elif UNITY_WEBGL
                return;
#else
            //  if (Application.platform == RuntimePlatform.OSXPlayer)
            //    return;
            //we're integrating with GOG again
            if (clientType == StoreClient.Gog && GogGalaxyManager.IsInitialized())
            {
                _gogClientProvider = new GOGStorefrontProvider();
               return;


            }
#endif
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
    }
}
