using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Assets.Scripts.Application.Entities;
using Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Services;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.NullObjects;
using SecretHistories.Services;
using SecretHistories.UI;
using SecretHistories.Elements;
using SecretHistories.Constants;
using SecretHistories.Constants.Modding;
using SecretHistories.Spheres;

using SecretHistories.Enums;
using SecretHistories.Infrastructure;
using SecretHistories.Infrastructure.Persistence;
using SecretHistories.Manifestations;
using UnityEngine;

namespace SecretHistories.Services
{

    public class Glory: MonoBehaviour
    {
#pragma warning disable 649


        public LanguageManager languageManager;
        public StageHand stageHand;
        

        public Concursum concursum;
        public SecretHistory SecretHistory;

        [SerializeField] private StorefrontServicesProvider storefrontServicesProvider;
        [SerializeField] private ScreenResolutionAdapter screenResolutionAdapter;
        [SerializeField] private GraphicsSettingsAdapter graphicsSettingsAdapter;
        [SerializeField] private WindowSettingsAdapter windowSettingsAdapter;
        [SerializeField] private SoundManager soundManager;
        public Limbo limbo;
        public NullManifestation NullManifestation;
        [SerializeField]private Stable _stable;

        [SerializeField] private string _gameId;

        public void SetGameIdFieldInEditor(GameId newGameId)
        {
            _gameId = newGameId.ToString();
        }

        private string _initialisedAt;
#pragma warning restore 649

        public Glory(ScreenResolutionAdapter screenResolutionAdapter)
        {
            this.screenResolutionAdapter = screenResolutionAdapter;
        }


        public void Awake()
        {
            if (_initialisedAt == null)
                _initialisedAt = DateTime.Now.ToString();
            else
            {
                NoonUtility.LogWarning("Problem: looks like we're trying to load the master scene twice");
                return;
            }

            NoonUtility.Subscribe(SecretHistory);
            var logWrapper=new UnityLogWrapper();
            NoonUtility.Subscribe(logWrapper);

            LogSystemSettings();
            //Glory.Initialise needs to be run before anything else... or oyu won't like what happens next.
            Initialise();
            
        }

        private void LogSystemSettings()
        {

            // log the current system settings
            string info = "Cultist Simulator Version: " + Application.version + "\n";
            info += "OS: " + SystemInfo.operatingSystem + "\n";
            info += "Processor: " + SystemInfo.processorType + " Count: " + SystemInfo.processorCount + "\n";
            info += "Graphics: " + SystemInfo.graphicsDeviceID + "/" + SystemInfo.graphicsDeviceName + "/" + SystemInfo.graphicsDeviceVendor + "/" + SystemInfo.graphicsDeviceVersion + "/" + SystemInfo.graphicsMemorySize + " Shader: " + SystemInfo.graphicsShaderLevel + "\n";
            info += "Memory: system - " + SystemInfo.systemMemorySize + " graphics - " + SystemInfo.graphicsMemorySize + "\n";

            NoonUtility.Log(info, 0);

        }
        
        private async void Initialise()
        {
            try
            {
                
                // force invariant culture to fix Linux save file issues
                System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

                var watchman = new Watchman();

                //Here we specify which game it is. This affects the content folder as set in the config next.
               GameId gameIdAsEnum = (GameId)Enum.Parse(typeof(GameId), _gameId);
                //load config: this gives us a lot of info that we'll need early
                var config = new Config();
                config.SetGame(gameIdAsEnum);
                    
                watchman.Register(config);
   
            
                //load concursum: central nexus for event responses
                watchman.Register(concursum);

                var metaInfo = new MetaInfo(gameIdAsEnum,new VersionNumber(Application.version),GetCurrentStorefront(),GetPersistentDataPath(gameIdAsEnum));
                watchman.Register<MetaInfo>(metaInfo);
            

                //stagehand is used to load scenes
                watchman.Register<StageHand>(stageHand);


                watchman.Register(limbo);
                watchman.Register(NullManifestation);
                

                //why here? why not? this whole thing needs fixing
                watchman.Register<IDice>(new Dice());

                //Set up storefronts: integration with GOG and Steam, so this should come early.

                if(metaInfo.Storefront==Storefront.Steam)
                    storefrontServicesProvider.InitialiseForStorefrontClientType(StoreClient.Steam);
                if (metaInfo.Storefront == Storefront.Gog)
                    storefrontServicesProvider.InitialiseForStorefrontClientType(StoreClient.Gog);
                watchman.Register<StorefrontServicesProvider>(storefrontServicesProvider);

                //set up the Mod Manager
                watchman.Register(new ModManager());
                //load Compendium content. We can't do anything with content files until this is in.
                watchman.Register<Compendium>(new Compendium());


                //LOAD DLLS HERE, ONCE ONLY
                Watchman.Get<ModManager>().CatalogueMods();
                Watchman.Get<ModManager>().LoadModDLLs();

                
                //yup, this is fine. content folder is set by game id, first thing.
                string contentFolder = Watchman.Get<Config>().GetConfigValue(NoonConstants.CONTENT_FOLDER_NAME_KEY);
                CompendiumLoader loader = new CompendiumLoader(contentFolder);
                watchman.Register<CompendiumLoader>(loader);

                var log=LoadCompendium(Watchman.Get<Config>().GetConfigValue(NoonConstants.CULTURE_SETTING_KEY));

                if (log.ImportFailed())
                { 
                    stageHand.LoadInfoScene();
                    return;
                }

                //setting defaults are set as the compendium is loaded, but they may also need to be
                //migrated from somewhere other than config (like PlayerPrefs)
                //so we only run this now, allowing it to overwrite any default values
                Watchman.Get<Config>().MigrateAnySettingValuesInRegistry(Watchman.Get<Compendium>());


                //set up loc services
                watchman.Register(languageManager);
                languageManager.Initialise();


                //respond to future culture-changed events, but not the initial one
                concursum.BeforeChangingCulture.AddListener(OnCultureChanged);
                
                var conversionTask= ConvertClassicSaveIfNecessary();
                await conversionTask;

                try
                {
                    var gamePersistenceProvider=GamePersistenceProvider.GetBestGuessGamePersistence();
     
                    gamePersistenceProvider.DepersistGameState();
                    var persistedState = gamePersistenceProvider.RetrievePersistedGameState();
                    var characterCreationCommand = persistedState.MostRecentCharacterCommand();
     
                    characterCreationCommand.ExecuteToProtagonist(_stable);

                    stageHand.UseProvider(gamePersistenceProvider);

                }
                catch (Exception e)
                {
                    NoonUtility.Log($"Couldn't initialise / depersist game state when setting up: {e}",2);
                }



                watchman.Register(_stable);


                //set up the top-level adapters. We do this here in case we've diverted to the error scene on first load / content fail, in order to avoid spamming the log with messages.
                screenResolutionAdapter.Initialise();
                graphicsSettingsAdapter.Initialise();
                windowSettingsAdapter.Initialise();
                soundManager.Initialise();

                string perpetualEditionDumbfileLocation = Application.streamingAssetsPath + "/edition/semper.txt";
                if (File.Exists(perpetualEditionDumbfileLocation))
                    NoonUtility.PerpetualEdition = true;


                // load the first scene and get the ball rolling.
                stageHand.LoadFirstScene(Watchman.Get<Config>().skiplogo);

  

            }
            catch (Exception e)
            {
                if (Application.isEditor)
                    throw;

                NoonUtility.LogException(e);
                stageHand.LoadInfoScene();
                
            }
        }

        protected string GetPersistentDataPath(GameId gameId)
        {
            string persistentPathFromUnity = Application.persistentDataPath;

            //NOT FOR PRODUCTION

            string persistentPath;
            if (gameId == Enums.GameId.BH)
                persistentPath = persistentPathFromUnity.Replace("Cultist Simulator", "Book of Hours");
            else
                persistentPath = persistentPathFromUnity;

            return persistentPath;
        }


        private async Task<bool> ConvertClassicSaveIfNecessary()
        {
            //convert save.txt to save.json if
            //(1) save.txt exists
            //(2) save.json doesn't.

            PetromnemeGamePersistenceProvider conversionProvider=new PetromnemeGamePersistenceProvider();
            if (!conversionProvider.SaveExists())
                return false;

            DefaultGamePersistenceProvider formatProvider2021=new DefaultGamePersistenceProvider();
            if (formatProvider2021.SaveExists())
                return false;
            
            var saveTask= conversionProvider.AttemptSaveConversion();
            await saveTask;

            return saveTask.Result;

        }


        public ContentImportLog LoadCompendium(string cultureId)
        {

            var log = Watchman.Get<CompendiumLoader>().PopulateCompendium(Watchman.Get<Compendium>(),cultureId);
            foreach (var m in log.GetMessages())
                NoonUtility.Log(m);

            return log;
        }

        private void OnCultureChanged(CultureChangedArgs args)
        {
            LoadCompendium(args.NewCulture.Id);
        }


        private  Storefront GetCurrentStorefront()
        {
            var storeFilePath = Path.Combine(Application.streamingAssetsPath, NoonConstants.STOREFRONT_PATH_IN_STREAMINGASSETS);
            if (!File.Exists(storeFilePath))
            {
                return Storefront.Unknown;
            }

            var edition = File.ReadAllText(storeFilePath).Trim().ToUpper();
            switch (edition)
            {
                case "STEAM":
                    return Storefront.Steam;
                case "GOG":
                    return Storefront.Gog;
                case "HUMBLE":
                    return Storefront.Humble;
                default:
                    return Storefront.Unknown;
            }
        }
    }
}
