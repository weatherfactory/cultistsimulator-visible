using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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


        public void Awake()
        {

            var registryAccess = new Registry();


            var storefrontServicesProvider = new StorefrontServicesProvider();
            storefrontServicesProvider.InitialiseForStorefrontClientType(StoreClient.Steam);
            storefrontServicesProvider.InitialiseForStorefrontClientType(StoreClient.Gog);
            registryAccess.Register<StorefrontServicesProvider>(storefrontServicesProvider);




            //TODO: make this async
            registryAccess.Register(new ModManager());
            registryAccess.Register<ICompendium>(new Compendium());

            var contentImporter = new CompendiumLoader();
            var log = contentImporter.PopulateCompendium(Registry.Retrieve<ICompendium>());

            foreach (var m in log.GetMessages())
                NoonUtility.Log(m);


            languageManager.Initialise(Registry.Retrieve<ICompendium>());
            registryAccess.Register<LanguageManager>(languageManager);

        }

    }
}
