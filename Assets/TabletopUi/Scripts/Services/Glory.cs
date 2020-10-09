using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Fucine;
using Assets.Core.Services;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
using Noon;
using TabletopUi.Scripts.Services;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Services
{
    public class Glory: MonoBehaviour
    {
        public LanguageManager languageManager;
        public StageHand stageHand;
        public PrefabFactory prefabFactory;

        public Concursum concursum;
        public SecretHistory SecretHistory;
        public Character Character;

        [SerializeField] private ScreenResolutionAdapter screenResolutionAdapter;
        [SerializeField] private GraphicsSettingsAdapter graphicsSettingsAdapter;
        [SerializeField] private WindowSettingsAdapter windowSettingsAdapter;
        [SerializeField] private SoundManager soundManager;
        [SerializeField] private Limbo limbo;


        private string initialisedAt = null;

        public Glory(ScreenResolutionAdapter screenResolutionAdapter)
        {
            this.screenResolutionAdapter = screenResolutionAdapter;
        }


        public void Awake()
        {
            if (initialisedAt == null)
                initialisedAt = DateTime.Now.ToString();
            else
            {
                NoonUtility.Log("Problem: looks like we're trying to load the master scene twice",2);
                return;
            }

            NoonUtility.Subscribe(SecretHistory);

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



        private void Initialise()
        {
            // force invariant culture to fix Linux save file issues
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;


            var registryAccess = new Registry();


            //load config: this gives us a lot of info that we'll need early
            registryAccess.Register(new Config());            
            
            //load concursum: central nexus for event responses
            registryAccess.Register(concursum);

          var metaInfo = new MetaInfo(new VersionNumber(Application.version),GetCurrentStorefront());
            registryAccess.Register<MetaInfo>(metaInfo);
            

            //stagehand is used to load scenes
            registryAccess.Register<StageHand>(stageHand);

            //prefabFactory means we don't have to rely on magic strings prefab names
            registryAccess.Register<PrefabFactory>(prefabFactory);

            //why here? why not? this whole thing needs fixing
            registryAccess.Register<IDice>(new Dice());

            //Set up storefronts: integration with GOG and Steam, so this should come early.
            var storefrontServicesProvider = new StorefrontServicesProvider();
            if(metaInfo.Storefront==Storefront.Steam)
                storefrontServicesProvider.InitialiseForStorefrontClientType(StoreClient.Steam);
            if (metaInfo.Storefront == Storefront.Gog)
                storefrontServicesProvider.InitialiseForStorefrontClientType(StoreClient.Gog);
            registryAccess.Register<StorefrontServicesProvider>(storefrontServicesProvider);

            //set up the Mod Manager
            registryAccess.Register(new ModManager());

            //load Compendium content. We can't do anything with content files until this is in.
            registryAccess.Register<ICompendium>(new Compendium());
            var log=LoadCompendium(Registry.Get<Config>().GetConfigValue(NoonConstants.CULTURE_SETTING_KEY));

            if (log.ImportFailed())
            {
                stageHand.LoadInfoScene();
                return;
            }

            //setting defaults are set as the compendium is loaded, but they may also need to be
            //migrated from somewhere other than config (like PlayerPrefs)
            //so we only run this now, allowing it to overwrite any default values
            Registry.Get<Config>().MigrateAnySettingValuesInRegistry(Registry.Get<ICompendium>());


            //set up loc services
            registryAccess.Register(languageManager);
            languageManager.Initialise();


            //respond to future culture-changed events, but not the initial one
            concursum.BeforeChangingCulture.AddListener(OnCultureChanged);

            //TODO: async
            LoadCurrentSaveOrCreateNewCharacter(registryAccess);

            var chronicler = new Chronicler(Registry.Get<Character>(), Registry.Get<ICompendium>());
            registryAccess.Register(chronicler);

            //set up the top-level adapters. We do this here in case we've diverted to the error scene on first load / content fail, in order to avoid spamming the log with messages.
            screenResolutionAdapter.Initialise();
            graphicsSettingsAdapter.Initialise();
            windowSettingsAdapter.Initialise();
            soundManager.Initialise();

            var stackManagersCatalogue = new TokenContainersCatalogue();
            registryAccess.Register(stackManagersCatalogue);


            //finally, load the first scene and get the ball rolling.
            stageHand.LoadFirstScene(Registry.Get<Config>().skiplogo);


        }

        private void LoadCurrentSaveOrCreateNewCharacter(Registry registry)
        {
            registry.Register(Character);


            var saveGameManager = new GameSaveManager(new GameDataImporter(Registry.Get<ICompendium>()), new GameDataExporter());

            if (saveGameManager.DoesGameSaveExist())
                saveGameManager.LoadCharacterState(SourceForGameState.DefaultSave,Character);
            

            

        }

        public ContentImportLog LoadCompendium(string cultureId)
        {
            var compendiumLoader = new CompendiumLoader();
            var log = compendiumLoader.PopulateCompendium(Registry.Get<ICompendium>(),cultureId);
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
