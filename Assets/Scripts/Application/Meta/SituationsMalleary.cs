using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Meta;
using SecretHistories.Assets.Scripts.Application.UI;
using SecretHistories.Commands;
using SecretHistories.Commands.Encausting;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Commands.TokenEffectCommands;
using SecretHistories.Constants.Events;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Events;
using SecretHistories.Fucine;
using SecretHistories.NullObjects;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Application.Meta
{
   public class SituationsMalleary: MonoBehaviour,ISphereEventSubscriber, ISituationSubscriber
   {
       [SerializeField] private AutoCompletingInput input;
       [SerializeField] private ThresholdSphere _situationDrydock;
       [SerializeField] private GameObject _linksPanel;
        [SerializeField] private LinkedRecipeDetailsDisplay _currentRecipeDetails;
       [SerializeField] private RecipeDetailsBrowser _altRecipeDetails;
       [SerializeField] private RecipeDetailsBrowser _linkedRecipeDetails;
       




        public void Awake()
       {
           var sphereSphec = new SphereSpec(typeof(ThresholdSphere), "drydock");
           sphereSphec.SetId("situationsmalleary");
           sphereSphec.Label = "Malleary: Situations";
           sphereSphec.AllowAnyToken = true;

           _situationDrydock.SetPropertiesFromSpec(sphereSphec);

           _situationDrydock.Subscribe(this);

           _linksPanel.SetActive(false);
       }



        public void CreateSituation()
       {

           var sh = new SerializationHelper();
           if (sh.MightBeJson(input.text))
           {
               var command = sh.DeserializeFromJsonString<TokenCreationCommand>(input.text);
               command.Execute(Context.Unknown(),_situationDrydock);
               return;
           }

           string entityId = input.text;

           var compendium = Watchman.Get<Compendium>();
           var recipe = compendium.GetEntityById<Recipe>(entityId.Trim());
           SituationCreationCommand newSituationCommand;
           if (recipe.IsValid())
           {
               newSituationCommand = new SituationCreationCommand().WithVerbId(recipe.ActionId).WithRecipeAboutToActivate(recipe.Id);
                //assuming we want the whole lifetime of the recipe
                  newSituationCommand.TimeRemaining = recipe.Warmup;
           }
           else
           {
               //if not a recipe, could it be a verb? create it, unstarted.
               var verb = compendium.GetEntityById<Verb>(entityId);
               if (verb.IsValid())
                   newSituationCommand =
                       new SituationCreationCommand().WithVerbId(verb.Id);
               else
                   return;
           }
           var newTokenLocation = new TokenLocation(0f, 0f, 0f, _situationDrydock.GetAbsolutePath()); //this is the only place we need to, or should, specify the path!
           var newTokenCommand = new TokenCreationCommand(newSituationCommand, newTokenLocation);
          var newToken= newTokenCommand.Execute(new Context(Context.ActionSource.Debug),_situationDrydock);


           EncaustDrydockedItem(newToken, input);
           PopulateLinksPanel(newToken.Payload);




        }

        public void TimeForwards()
        {
            _situationDrydock.RequestTokensSpendTime(Heart.BEAT_INTERVAL_SECONDS * 50, 0);
        }

        public void TimeBackwards()
        {
            _situationDrydock.RequestTokensSpendTime(Heart.BEAT_INTERVAL_SECONDS * -50, 0);
        }

        public void AdvanceTimeToEndOfRecipe()
        {
            var situationToken = _situationDrydock.GetTokenInSlot();
            if(situationToken.Payload!=null)
            {
            var timeRemaining = situationToken.Payload.GetTimeshadow().LifetimeRemaining;
            if(timeRemaining>0)
                _situationDrydock.RequestTokensSpendTime(timeRemaining, 0);
            }
        }

        public void AddNote()
        {
            var situation = _situationDrydock.GetTokenInSlot().Payload;
            string title = "!";
            string description = input.text;
   
            var note=new Notification(title, description);


            var addNoteCommand=new AddNoteToTokenCommand(note,new Context(Context.ActionSource.UI));

          situation.ExecuteTokenEffectCommand(addNoteCommand);
        }

        public void DestroyDrydockedToken()
        {
            var token = _situationDrydock.GetTokenInSlot();
            if(token!=null)
                token.Retire(RetirementVFX.None);
        }

        public void OnSphereChanged(SphereChangedArgs args)
        {
            //
        }

        public void OnTokensChangedForSphere(SphereContentsChangedEventArgs args)
        {
            if(args.TokenRemoved!=null)
            {
                ClearLinksPanel();
                if (args.TokenRemoved.Payload is Situation situation)
                    situation.RemoveSubscriber(this);
            }
            else if(args.TokenAdded!=null)
            
            {
                EncaustDrydockedItem(args.TokenAdded, input);
                PopulateLinksPanel(args.TokenAdded.Payload);

                if(args.TokenAdded.Payload is Situation situation)
                    situation.AddSubscriber(this);
            }
        }
        

        public void OnTokenInteractionInSphere(TokenInteractionEventArgs args)
        {
//
        }

        public void EncaustDrydockedItem(Token drydockedItem, AutoCompletingInput jsonEditField)
        {
            try
            {

            
            var encaustery = new Encaustery<TokenCreationCommand>();
            var encaustedCommand = encaustery.Encaust(drydockedItem);
            var sh = new SerializationHelper();

            jsonEditField.text = sh.SerializeToJsonString(encaustedCommand);
            }
            catch (Exception e)
            {
                NoonUtility.LogWarning($"Couldn't encaust drydocked item: {e.Message}");
                jsonEditField.text = string.Empty;
            }
        }

        private void PopulateLinksPanel(ITokenPayload payload)
        {
            if (!(payload is Situation situation))
                return;

            _linksPanel.SetActive(true);

            Recipe recipeToAnalyze = situation.Recipe;
            if (!recipeToAnalyze.IsValid())
            {
                recipeToAnalyze = Watchman.Get<Compendium>()
                    .GetEntityById<Recipe>(situation.CurrentRecipePrediction.RecipeId);

                //conceivably this could still be a null recipe, in which everything will stay blank.
            }


            _currentRecipeDetails.Populate(LinkedRecipeDetails.AsCurrentRecipe(recipeToAnalyze),situation);

            _altRecipeDetails.PopulateLinks(recipeToAnalyze.Alt,situation);
            _linkedRecipeDetails.PopulateLinks(recipeToAnalyze.Linked, situation);
        }

        private void ClearLinksPanel()
        {
            _currentRecipeDetails.Populate(LinkedRecipeDetails.AsCurrentRecipe(NullRecipe.Create()),NullSituation.Create());
            _altRecipeDetails.Clear();
            _linkedRecipeDetails.Clear();
            _linksPanel.SetActive(false);
        }

        public void SituationStateChanged(Situation s)
        {
            EncaustDrydockedItem(s.Token, input);
            PopulateLinksPanel(s);

        }

        public void TimerValuesChanged(Situation s)
        {
            EncaustDrydockedItem(s.Token, input);
            PopulateLinksPanel(s);
        }

        public void SituationSphereContentsUpdated(Situation s)
        {
            EncaustDrydockedItem(s.Token, input);
            PopulateLinksPanel(s);
        }
   }
}
