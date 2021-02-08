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
        [SerializeField] private InputField input;
        [SerializeField] private TMP_InputField jsonInputField;
        private ThresholdSphere primaryThreshold;

        public void Awake()
        {
            primaryThreshold=elementDrydockWrangler.BuildPrimaryThreshold(new SphereSpec(), SituationPath.Root(), new NullVerb());
            primaryThreshold.Subscribe(this);
        }

        public void CreateDrydockedItem()
        {
            var elementId = input.text;

            
            var tabletopPath =
                new SpherePath(Watchman.Get<Compendium>().GetSingleEntity<Dictum>().DefaultWorldSpherePath);
            var tabletop = Watchman.Get<SphereCatalogue>().GetSphereByPath(tabletopPath);
            var element = Watchman.Get<Compendium>().GetEntityById<Element>(elementId);

            if (element.Id == NullElement.Create().Id)
                return;

            Context debugContext = new Context(Context.ActionSource.Debug);

            primaryThreshold.ModifyElementQuantity(elementId, 1, debugContext);

            EncaustDrydockedItem(primaryThreshold.GetTokenInSlot(),jsonInputField);

        }

        public void DestroyDrydockedItem()
        {

            var elementId = input.text;
            primaryThreshold.ModifyElementQuantity(elementId, -1, new Context(Context.ActionSource.Debug));
        }

        public void Mutate()
        {
            var elementToken = primaryThreshold.GetElementTokenInSlot();
            elementToken.Payload.SetMutation(input.text,1,true);
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
                EncaustDrydockedItem(args.Token,jsonInputField);
            }
        }

        public void EncaustDrydockedItem(Token drydockedItem, TMP_InputField jsonEditField)
        {
            var encaustery=new Encaustery<TokenCreationCommand>();
            var encaustedCommand= encaustery.Encaust(drydockedItem);
            var serializerFactory = new SerializationHelper();

            var sh=new SerializationHelper();

            jsonEditField.text = sh.SerializeToJsonString(encaustedCommand);
        }
    }
}
