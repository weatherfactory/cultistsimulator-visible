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
using SecretHistories.Constants.Events;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Events;
using SecretHistories.Fucine;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Application.Meta
{
   public class SituationsMalleary: MonoBehaviour,ISphereEventSubscriber
   {
       [SerializeField] private AutoCompletingInput input;
       [SerializeField] private ThresholdSphere _situationDrydock;
       [SerializeField] private GameObject _linksPanel;
        [SerializeField] private LinkedRecipeDetailsBrowser _altRecipeDetails;
       [SerializeField] private LinkedRecipeDetailsBrowser _linkedRecipeDetails;



        public void Awake()
       {
           var sphereSphec = new SphereSpec(typeof(ThresholdSphere), "drydock");
           sphereSphec.SetId("situationsmalleary");
           sphereSphec.Label = "Malleary: Situations";
           sphereSphec.AllowAnyToken = true;

           _situationDrydock.ApplySpec(sphereSphec);

            _situationDrydock.Subscribe(this);
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
            var situation = _situationDrydock.GetTokenInSlot().Payload;
            var timeRemaining = situation.GetTimeshadow().LifetimeRemaining;
            if(timeRemaining>0)
                _situationDrydock.RequestTokensSpendTime(timeRemaining, 0);
        }

        public void AddNote()
        {
            var situation = _situationDrydock.GetTokenInSlot().Payload;
            string title = "!";
            string description = input.text;
   
            var note=new Notification(title, description);


            var addNoteCommand=new AddNoteCommand(note,new Context(Context.ActionSource.UI));

          situation.ExecuteTokenEffectCommand(addNoteCommand);
        }

        public void DestroyDrydockedToken()
        {
            var token = _situationDrydock.GetTokenInSlot();
            token.Retire(RetirementVFX.None);
        }

        public void OnSphereChanged(SphereChangedArgs args)
        {
            //
        }

        public void OnTokensChangedForSphere(SphereContentsChangedEventArgs args)
        {
            if(args.TokenRemoved!=null)
               ClearLinksPanel();
        }

        public void OnTokenInteractionInSphere(TokenInteractionEventArgs args)
        {
            if (args.Interaction == Interaction.OnDragEnd)
            {
                input.text = args.Payload.EntityId;
                EncaustDrydockedItem(args.Token, input);
                PopulateLinksPanel(args.Payload);
            }
        }

        public void EncaustDrydockedItem(Token drydockedItem, AutoCompletingInput jsonEditField)
        {
            var encaustery = new Encaustery<TokenCreationCommand>();
            var encaustedCommand = encaustery.Encaust(drydockedItem);
            var sh = new SerializationHelper();

            jsonEditField.text = sh.SerializeToJsonString(encaustedCommand);
        }

        private void PopulateLinksPanel(ITokenPayload payload)
        {
            if (!(payload is Situation situation))
                return;

            

            _altRecipeDetails.PopulateLinks(situation.Recipe.Alt);
        }

        private void ClearLinksPanel()
        {

            _altRecipeDetails.Clear();
        }
    }
}
