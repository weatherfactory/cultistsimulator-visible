using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
using Assets.TabletopUi.Scripts.Services;
using Noon;
using Steamworks;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public class SteamworksStorefrontClientProvider : IStoreFrontClientProvider
    {
      
        private CGameID _gameId;
        private CSteamID _steamId;

        private CallResult<CreateItemResult_t> r_itemCreated;
        private CallResult<SubmitItemUpdateResult_t> r_itemUpdateCompleted;

        private static Mod _currentlyUploadingMod=new NullMod();

        private void SetCurrentlyUploadingMod(Mod mod)
        {
            _currentlyUploadingMod = mod;
        }

        private void ClearCurrentlyUploadingMod()
        {
            _currentlyUploadingMod=new NullMod();
        }


        public SteamworksStorefrontClientProvider()
        {
            if (!SteamManager.Initialized)
                return;

            // Cache the GameID for use in the Callbacks
            _gameId = new CGameID(SteamUtils.GetAppID());
            _steamId=new CSteamID();
            
            // Set up Steam callbacks
            Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);

            r_itemCreated = CallResult<CreateItemResult_t>.Create(OnWorkshopItemCreated);
            r_itemUpdateCompleted = CallResult<SubmitItemUpdateResult_t>.Create(OnWorkshopItemUpdateCompleted);

            // Fetch user data
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
                SteamUserStats.StoreStats();
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


        public void UploadMod(Mod modToUpload)
        {
            if(SteamworksStorefrontClientProvider._currentlyUploadingMod.IsValid)
            {
                NoonUtility.Log("Already uploading mod: " + _currentlyUploadingMod.Id);
                return;
            }

            SetCurrentlyUploadingMod(modToUpload);

            //make a call to the API and give it a handle
            SteamAPICall_t handle = SteamUGC.CreateItem(_gameId.AppID(),
                EWorkshopFileType.k_EWorkshopFileTypeCommunity);

            //associate the previously created call result with it
            r_itemCreated.Set(handle);
            //and when it's completed, the call result has a delegate that it calls in turn

        }


        public void UpdateMod(Mod modToUpdate, string publishedFileId)
        {
            if(SteamworksStorefrontClientProvider._currentlyUploadingMod.IsValid)
            {
                NoonUtility.Log("Already uploading mod: " + _currentlyUploadingMod.Id);
                return;
            }

            try
            {

                var ulongPublishedFileId = Convert.ToUInt64((publishedFileId));

                SetCurrentlyUploadingMod(modToUpdate);


                StartItemUpdate(new PublishedFileId_t(ulongPublishedFileId));
            }
        
            catch (Exception e)
            {
                NoonUtility.Log("Disaster! invalid published file id: " + publishedFileId);

            }


        }

        private void OnWorkshopItemCreated(CreateItemResult_t callback, bool ioFailure)
        {
            if(callback.m_bUserNeedsToAcceptWorkshopLegalAgreement)
            {

                var concursum = Registry.Retrieve<Concursum>();

                concursum.ShowNotification(new NotificationArgs
                {
                    Title = "Steam Workshop Terms",
                    Description =
                        "Steam reckons you haven't yet accepted the Steam Workshop terms. You can upload this item, but it'll remain hidden until you've accepted the terms on the Workshop page."
                });
            }
            else
                NoonUtility.Log("User has accepted Steam Workshop legal agreement; continuing upload");

            StartItemUpdate(callback.m_nPublishedFileId);
        }

        private void StartItemUpdate(PublishedFileId_t callbackPublishedFileId)
        {
            UGCUpdateHandle_t updateHandle = SteamUGC.StartItemUpdate(_gameId.AppID(),
                callbackPublishedFileId);

            var modToUpload = _currentlyUploadingMod;

            if (!modToUpload.IsValid)
            {
                NoonUtility.Log($"Created fileid for a new mod {callbackPublishedFileId}, but there's no mod to upload, or it's otherwise invalid",1);
                return;
            }

            SteamUGC.SetItemTitle(updateHandle, modToUpload.Name);
            SteamUGC.SetItemDescription(updateHandle, modToUpload.Description);
            SteamUGC.SetItemContent(updateHandle, modToUpload.ModRootFolder);


            SteamAPICall_t updateCompleteHandle = SteamUGC.SubmitItemUpdate(updateHandle, "update at: " + DateTime.Now);
            r_itemUpdateCompleted.Set(updateCompleteHandle);
        }


        private void OnWorkshopItemUpdateCompleted(SubmitItemUpdateResult_t callback, bool IOFailure)
        {

            if (IOFailure)
            {
                NoonUtility.Log($"ARGGKKKK IO UPDATE FAILURE FOR MOD UPLOAD {callback.m_nPublishedFileId}, {_currentlyUploadingMod.Id}");
            }
            else
            {
                NoonUtility.Log($"Update completed for item {callback.m_nPublishedFileId}, mod {_currentlyUploadingMod.Id} with result {callback.m_eResult}");
            }
            ModUploadedArgs args = new ModUploadedArgs { Mod=_currentlyUploadingMod, PublishedFileId = callback.m_nPublishedFileId.ToString() };

            Registry.Retrieve<Concursum>().ModUploadedEvent.Invoke(args);


           ClearCurrentlyUploadingMod();


              SteamFriends.ActivateGameOverlayToWebPage($"steam://url/CommunityFilePage/{callback.m_nPublishedFileId}");

        }


        


        public List<SubscribedStorefrontMod> GetSubscribedItems()
        {
    List<SubscribedStorefrontMod> subscribedStorefrontMods=new List<SubscribedStorefrontMod>();     

           uint numberOfSubscribedItems=SteamUGC.GetNumSubscribedItems(); //I'm not really sure why this is working without an async call? is it getting it interprocess?
           

           PublishedFileId_t[] subscribedItems = new PublishedFileId_t[numberOfSubscribedItems];

           UInt32 numberRetrieved=SteamUGC.GetSubscribedItems(subscribedItems, numberOfSubscribedItems);


           var listSubscribedItems = subscribedItems.ToList();

           foreach (PublishedFileId_t subscribedItem in listSubscribedItems)

           {

               UInt64 fileSize;
               string pchFolder;
               UInt32 pchFolderSize = 1024;
               UInt32 punTimeStamp;

               var itemIsInstalled = SteamUGC.GetItemInstallInfo(subscribedItem, out fileSize, out pchFolder,
                   pchFolderSize, out punTimeStamp);
               if (itemIsInstalled)
               {

                     SubscribedStorefrontMod installedMod = new SubscribedStorefrontMod {ModRootFolder = pchFolder};
                     subscribedStorefrontMods.Add(installedMod);
               }
           }

           return subscribedStorefrontMods;

        }


    }
    }

