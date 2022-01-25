using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Constants;
using SecretHistories.Constants.Modding;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Infrastructure;
using SecretHistories.Infrastructure.Persistence;
using SecretHistories.Services;
using SecretHistories.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Application.Meta
{
    public class MiscMalleary : MonoBehaviour
    {
        [SerializeField] private AutoCompletingInput input;
        [SerializeField] public TextMeshProUGUI ResponseLabel;

        [SerializeField] private Button MetapauseButton;
        [SerializeField] private Button UnmetapauseButton;
        [SerializeField] private Button SetCheevo;
        [SerializeField] private Button ClearCheevo;


        public void Start()
        {
            SetCheevo.gameObject.SetActive(Watchman.Get<Config>().knock);
            ClearCheevo.gameObject.SetActive(Watchman.Get<Config>().knock);

            if (Watchman.Get<Heart>().Metapaused) //in case we're metapaused when the console is enabled
            {
                MetapauseButton.gameObject.SetActive(false);
                UnmetapauseButton.gameObject.SetActive(true);
            }
        }

        void HaltVerb(string verbId)
        {
            Watchman.Get<HornedAxe>().HaltSituation(verbId, 1);

        }

        private void DeleteVerb(string verbId)
        {
            Watchman.Get<HornedAxe>().PurgeSituation(verbId, 1);
        }

        public void PurgeElement()
        {
            Watchman.Get<HornedAxe>().PurgeElement(input.text, 1);

        }

        public void FindByPath()
        {
            var path = new FucinePath(input.text);
            if (!path.IsValid())
                ResponseLabel.text = $"Invalid Fucine path! {input.text}";

            if (path.GetEndingPathPart().Category == FucinePathPart.PathCategory.Sphere)
            {

                var sphereToFind = Watchman.Get<HornedAxe>().GetSphereByPath(path);
                if (sphereToFind != null)
                {
                    ResponseLabel.text = $"found sphere at {sphereToFind.GetAbsolutePath()}";
                    return;
                }

                ResponseLabel.text = $"couldn't find sphere: {input.text}";
                return;
            }

            if (path.GetEndingPathPart().Category == FucinePathPart.PathCategory.Token)
            {
                var tokenToFind = Watchman.Get<HornedAxe>()
                    .FindSingleOrDefaultTokenById(path.GetEndingPathPart().GetId());

                if (tokenToFind != null)
                {
                    tokenToFind.ShowPossibleInteraction();
                    ResponseLabel.text = $"found token at {tokenToFind.Location.AtSpherePath}{tokenToFind.PayloadId}";
                    return;
                }

                ResponseLabel.text = $"couldn't find token: {input.text}";
                return;
            }

            ResponseLabel.text = $"couldn't find path: {input.text}";
        }

    


        void BeginLegacy(string legacyId)
        {
            var l = Watchman.Get<Compendium>().GetEntityById<Legacy>(legacyId);
            if (l == null)
                return;

        }

       public void TriggerAchievement()
        {

            var storefrontServicesProvider = Watchman.Get<StorefrontServicesProvider>();
            storefrontServicesProvider.SetAchievementForCurrentStorefronts(input.text.ToUpperInvariant(), true);
        }

       public void ResetAchievement()
        {
            var storefrontServicesProvider = Watchman.Get<StorefrontServicesProvider>();
            storefrontServicesProvider.SetAchievementForCurrentStorefronts(input.text.ToUpperInvariant(), false);
        }


        public void UpdateCompendiumContent()
        {
            Watchman.Get<ModManager>().CatalogueMods();

            var existingCompendium = Watchman.Get<Compendium>();
            var compendiumLoader = new CompendiumLoader(Watchman.Get<Config>().GetConfigValue(NoonConstants.CONTENT_FOLDER_NAME_KEY));

            var startImport = DateTime.Now;
            var log = compendiumLoader.PopulateCompendium(existingCompendium, Watchman.Get<Config>().GetConfigValue(NoonConstants.CULTURE_SETTING_KEY));
            foreach (var m in log.GetMessages())
                NoonUtility.Log(m.Description, m.MessageLevel);

            NoonUtility.Log("Total time to import: " + (DateTime.Now - startImport));


        }

        public void WordCount()
        {
            var compendium = Watchman.Get<Compendium>();
            var log = new ContentImportLog();
            compendium.CountWords(log);
            foreach (var m in log.GetMessages())
                NoonUtility.Log(m.Description, m.MessageLevel);

        }

        public void FnordCount()
        {
            var compendium = Watchman.Get<Compendium>();
            var log = new ContentImportLog();
            compendium.LogFnords(log);
            foreach (var m in log.GetMessages())
                NoonUtility.Log(m.Description, m.MessageLevel);

        }

        public void ImageCheck()
        {

            var compendium = Watchman.Get<Compendium>();
            var log = new ContentImportLog();
            compendium.LogMissingImages(log);
            foreach (var m in log.GetMessages())
                NoonUtility.Log(m.Description, m.MessageLevel);
        }



        public void ApplyEndingFromInput()
        {
            Ending ending;
            string endingId = input.text;
            var compendium = Watchman.Get<Compendium>();

            ending = compendium.GetEntityById<Ending>(endingId);
            if(!ending.IsValid())
                ending = compendium.GetEntitiesAsList<Ending>().First();
                


                    //get a plausible token

                    var focalToken=Watchman.Get<HornedAxe>().GetDefaultSphere().GetTokensWhere(t => !t.Defunct).FirstOrDefault();
            if(focalToken==null)
                NoonUtility.LogWarning("Can't find any tokens in the default sphere to focus game ending on.");
            else
                Watchman.Get<GameGateway>().EndGame(ending,focalToken);



        }

        public void LoadGame()
        {
            Watchman.Get<StageHand>().LoadGameOnTabletop(new DefaultGamePersistenceProvider());
        }

        public async void SaveGame()
        {
            Watchman.Get<Heart>().Metapause();
            Watchman.Get<LocalNexus>().DisablePlayerInput(0f);


            var gameGateway =Watchman.Get<GameGateway>();
          var saveResult= await gameGateway.TryDefaultSave();

            if(saveResult)
            {
                Watchman.Get<Heart>().Unmetapause();
                Watchman.Get<LocalNexus>().EnablePlayerInput();
            }
            //if not, we'll let the gamegateway show a problem window and sort it out

        }

        public void Metapause()
        {
            Watchman.Get<Heart>().Metapause();
            MetapauseButton.gameObject.SetActive(false);
            UnmetapauseButton.gameObject.SetActive(true);
        }

        public void Unmetapause()
        {
            Watchman.Get<Heart>().Unmetapause();
            MetapauseButton.gameObject.SetActive(true);
            UnmetapauseButton.gameObject.SetActive(false);

        }

        public void DoHeartbeats(int beatsToDo)
        {
            Watchman.Get<Heart>().Metapause();

            int beatsDone = 0;
            while(beatsDone<beatsToDo)
            {
              Watchman.Get<Heart>().Beat(1f,0f);
              beatsDone++;
            }
            Watchman.Get<Heart>().Unmetapause();

        }

        public async void PetromnemeConversion()
        {
           var petro=new PetromnemeGamePersistenceProvider();
        petro.AttemptSaveConversion();
        }


        void QueueRoll(string roll)
        {
            //int rollValue;
            //int.TryParse(roll, out rollValue);
            //if (rollValue >= 1 && rollValue <= 100)
            //    QueuedRollsList.Add(rollValue);

            //UpdatedQueuedRollsDisplay();

        }

        public void UpdatedQueuedRollsDisplay()
        {
            //rollsQueued.text = string.Empty;
            //foreach (var i in QueuedRollsList)
            //{
            //    if (rollsQueued.text != "")
            //        rollsQueued.text += ", ";

            //    rollsQueued.text += i.ToString();
            //}
        }

        //public int PopNextOverrideValue(Recipe recipe = null)
        //{
        //    if (!QueuedRollsList.Any())
        //        return 0;
        //    else
        //    {
        //        int result = QueuedRollsList.First();
        //        QueuedRollsList.RemoveAt(0);
        //        UpdatedQueuedRollsDisplay();
        //        return result;
        //    }
        //}




    }
}
