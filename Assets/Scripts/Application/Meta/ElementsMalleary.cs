using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Commands.SituationCommands;
using Newtonsoft.Json;
using SecretHistories.Commands;
using SecretHistories.Commands.Encausting;
using SecretHistories.Constants.Events;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Interfaces;
using SecretHistories.NullObjects;
using SecretHistories.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Application.Meta
{
    public class ElementsMalleary: MonoBehaviour,ISphereEventSubscriber
    {
        [SerializeField] private ThresholdsWrangler elementDrydockWrangler;
        [SerializeField] private AutoCompletingInput input;
        private ThresholdSphere primaryThreshold;

        public void Awake()
        {
            primaryThreshold=elementDrydockWrangler.BuildPrimaryThreshold(new SphereSpec(), SituationPath.Root(), new NullVerb());
            primaryThreshold.Subscribe(this);
        }

        public void CreateDrydockedItem()
        {
            var sh=new SerializationHelper();
            if(sh.MightBeJson(input.text))
            {
                var command = sh.DeserializeFromJsonString<TokenCreationCommand>(input.text);
                command.Execute(Context.Unknown());
            }
            else
            {
                var elementId = input.text;
                var element = Watchman.Get<Compendium>().GetEntityById<Element>(elementId);

                if (element.Id == NullElement.Create().Id)
                    return;

                Context debugContext = new Context(Context.ActionSource.Debug);

                primaryThreshold.ModifyElementQuantity(elementId, 1, debugContext);

                EncaustDrydockedItem(primaryThreshold.GetTokenInSlot(), input);
            }
        }

        public void DestroyDrydockedItem()
        {
             primaryThreshold.GetTokenInSlot().Retire(RetirementVFX.CardTakenShadow);
        }

        public void Mutate()
        {
            var elementToken = primaryThreshold.GetElementTokenInSlot();
            elementToken.Payload.SetMutation(input.text, 1,true);
        }

        public void Unmutate()
        {
            var elementToken = primaryThreshold.GetElementTokenInSlot();
            elementToken.Payload.SetMutation(input.text, -1, true);
        }

        public void OnTokensChangedForSphere(SphereContentsChangedEventArgs args)
        {
//
        }

        public void OnTokenInteractionInSphere(TokenInteractionEventArgs args)
        {
            if (args.Interaction == Interaction.OnDragEnd)
            {
                input.text = args.Payload.Id;
                EncaustDrydockedItem(args.Token, input);
            }
        }


        public void EncaustDrydockedItem(Token drydockedItem, AutoCompletingInput jsonEditField)
        {
            var encaustery=new Encaustery<TokenCreationCommand>();
            var encaustedCommand= encaustery.Encaust(drydockedItem);
            var sh=new SerializationHelper();

            jsonEditField.text = sh.SerializeToJsonString(encaustedCommand);
        }
    }
}
