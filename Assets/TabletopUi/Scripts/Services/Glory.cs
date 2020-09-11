using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
using Noon;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Services
{
    public class Glory: MonoBehaviour
    {
        [SerializeField]
        public LanguageManager languageManager;
        [SerializeField]
        public StageHand stageHand;

        [SerializeField] public Concursum concursum;

        private string initialisedAt = null;


        public void Awake()
        {
            if (initialisedAt == null)
                initialisedAt = DateTime.Now.ToString();
            else
            {
                NoonUtility.Log("Problem: looks like we're trying to load the master scene twice",2);
                return;
            }

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
            registryAccess.Register(new Config(NoonConstants.DEFAULT_CULTURE_ID));            
            
            //load concursum: central nexus for event responses
            registryAccess.Register(concursum);

            //right now, this is just the version number
            var metaInfo = new MetaInfo(new VersionNumber(Application.version));
            registryAccess.Register<MetaInfo>(metaInfo);
            

            //stagehand is used to load scenes
            registryAccess.Register<StageHand>(stageHand);

        //why here? why not? this whole thing needs fixing
            registryAccess.Register<IDice>(new Dice());

            //Set up storefronts: integration with GOG and Steam, so this should come early.
            var storefrontServicesProvider = new StorefrontServicesProvider();
            storefrontServicesProvider.InitialiseForStorefrontClientType(StoreClient.Steam);
            storefrontServicesProvider.InitialiseForStorefrontClientType(StoreClient.Gog);
            registryAccess.Register<StorefrontServicesProvider>(storefrontServicesProvider);

            //set up the Mod Manager
            registryAccess.Register(new ModManager());

            //load Compendium content. We can't do anything with content files until this is in.
            registryAccess.Register<ICompendium>(new Compendium());
            ReloadCompendium(Registry.Get<Config>().CultureId);


            //setting defaults need to be set here, but they may be migrated from somewhere other than config (like PlayerPrefs) or
            //they may be set from /settings in JSON -so we can only do this once we have the Compendium loaded.
            //Registry.Get<Config>().MigrateOrSetDefaults(Registry.Get<ICompendium>());


            //set up loc services
            registryAccess.Register(languageManager);
            languageManager.Initialise();


            //get Concursum to respond to future culture-changed events, but not the initial one
            concursum.CultureChangedEvent.AddListener(OnCultureChanged);

            //TODO: async
            LoadCurrentSave(registryAccess);

            stageHand.LoadFirstScene(Registry.Get<Config>().skiplogo);


        }

        private void LoadCurrentSave(Registry registry)
        {


            var saveGameManager = new GameSaveManager(new GameDataImporter(Registry.Get<ICompendium>()), new GameDataExporter());

            if (saveGameManager.DoesGameSaveExist())
                registry.Register(saveGameManager.LoadCharacterState(SourceForGameState.DefaultSave));
            else
                registry.Register(new Character());


        }

        public void ReloadCompendium(string cultureId)
        {
            var compendiumLoader = new CompendiumLoader();
            var log = compendiumLoader.PopulateCompendium(Registry.Get<ICompendium>(),cultureId);
            foreach (var m in log.GetMessages())
                NoonUtility.Log(m);
        }

        private void OnCultureChanged(CultureChangedArgs args)
        {
            ReloadCompendium(args.NewCulture.Id);
        }
    }
}
