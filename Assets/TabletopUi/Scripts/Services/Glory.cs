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

            var config = new Config();
            config.ReadFromIniFile();

            registryAccess.Register(new Concursum());

            Registry.Get<Concursum>().SupplyConfig(config);



            var metaInfo = new MetaInfo(new VersionNumber(Application.version));
            registryAccess.Register<MetaInfo>(metaInfo);


            registryAccess.Register<StageHand>(stageHand);


            var storefrontServicesProvider = new StorefrontServicesProvider();
            storefrontServicesProvider.InitialiseForStorefrontClientType(StoreClient.Steam);
            storefrontServicesProvider.InitialiseForStorefrontClientType(StoreClient.Gog);

            registryAccess.Register<StorefrontServicesProvider>(storefrontServicesProvider);


            registryAccess.Register(new ModManager());
            registryAccess.Register<ICompendium>(new Compendium());

            ReloadCompendium(Registry.Get<Concursum>().GetCurrentCultureId());

            registryAccess.Register<ILanguageManager>(languageManager);
            languageManager.Initialise();


            Registry.Get<Concursum>().CultureChangedEvent.AddListener(OnCultureChanged);

            //TODO: async
            LoadCurrentSave(registryAccess);

            stageHand.LoadFirstScene(Registry.Get<Concursum>().GetSkipLogo());


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
