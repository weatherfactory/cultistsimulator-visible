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
using SecretHistories.Infrastructure.Persistence;
using SecretHistories.Services;
using SecretHistories.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Application.Meta
{
    public class MiscMalleary: MonoBehaviour
    {

        [SerializeField] private Button MetapauseButton;
        [SerializeField] private Button UnmetapauseButton;

        void HaltVerb(string verbId)
        {
            Watchman.Get<HornedAxe>().HaltSituation(verbId, 1);

        }

        private void DeleteVerb(string verbId)
        {
            Watchman.Get<HornedAxe>().DeleteSituation(verbId, 1);
        }

        private void PurgeElement(string elementId)
        {
            Watchman.Get<HornedAxe>().PurgeElement(elementId, 1);
        }


        void BeginLegacy(string legacyId)
        {
            var l = Watchman.Get<Compendium>().GetEntityById<Legacy>(legacyId);
            if (l == null)
                return;

        }

        void TriggerAchievement(string achievementId)
        {
            var storefrontServicesProvider = Watchman.Get<StorefrontServicesProvider>();
            storefrontServicesProvider.SetAchievementForCurrentStorefronts(achievementId, true);
        }

        void ResetAchievement(string achievementId)
        {
            var storefrontServicesProvider = Watchman.Get<StorefrontServicesProvider>();
            storefrontServicesProvider.SetAchievementForCurrentStorefronts(achievementId, false);
        }

        void FastForward(float interval)
        {
       throw new NotImplementedException();
       throw new NotImplementedException();
        }

        void UpdateCompendiumContent()
        {
            Watchman.Get<ModManager>().CatalogueMods();

            var existingCompendium = Watchman.Get<Compendium>();
            var compendiumLoader = new CompendiumLoader(Watchman.Get<Config>().GetConfigValue(NoonConstants.CONTENT_FOLDER_NAME_KEY));

            var startImport = DateTime.Now;
            var log = compendiumLoader.PopulateCompendium(existingCompendium, Watchman.Get<Config>().GetConfigValue(NoonConstants.CULTURE_SETTING_KEY));
            foreach (var m in log.GetMessages())
                NoonUtility.Log(m.Description, m.MessageLevel);

            NoonUtility.Log("Total time to import: " + (DateTime.Now - startImport));

            // Populate current decks with new cards (this will shuffle the deck)
            //Watchman.Get<Stable>().Protag().ResetStartingDecks();

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



        public void EndGame(string endingId)
        {
            var compendium = Watchman.Get<Compendium>();

            var ending = compendium.GetEntityById<Ending>(endingId);
            if (ending == null)
                ending = compendium.GetEntitiesAsList<Ending>().First();

            var endingPortal = Portal.CreateEndingPortal(ending,"anotherdescent");

            

            var endingIngressCreationCommand=new IngressCreationCommand(endingPortal);
           var tokenCreationCommand = new TokenCreationCommand(endingIngressCreationCommand,TokenLocation.Default(FucinePath.Root()));
           tokenCreationCommand.Execute(Context.Unknown(), Watchman.Get<HornedAxe>().GetDefaultSphere());

        }

        public void LoadGame()
        {
            Watchman.Get<StageHand>().LoadGameOnTabletop(new DefaultGamePersistenceProvider());
        }

        public async void SaveGame()
        {

            var game = new DefaultGamePersistenceProvider();
            game.Encaust(Watchman.Get<Stable>(), Watchman.Get<HornedAxe>());
            var saveTask = game.SerialiseAndSaveAsync();
            var result = await saveTask;

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
            Watchman.Get<Heart>().Beat(1f,1f);
            beatsDone++;
            }
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
