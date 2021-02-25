using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Spheres;
using Newtonsoft.Json;
using SecretHistories.Commands;
using SecretHistories.Commands.Encausting;
using SecretHistories.Constants.Events;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.NullObjects;
using SecretHistories.Spheres;
using SecretHistories.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Application.Meta
{
    public class ElementsMalleary: MonoBehaviour,ISphereEventSubscriber
    {
        [SerializeField] private SpheresWrangler elementDrydockWrangler;
        [SerializeField] private AutoCompletingInput input;
        private ThresholdSphere _drydockThresholdSphere;

        public void Awake()
        {
            var mallearyThresholdSpec = new SphereSpec(typeof(ThresholdSphere), "elementsmalleary");

            _drydockThresholdSphere =elementDrydockWrangler.BuildPrimarySphere(mallearyThresholdSpec, NullVerb.Create()) as ThresholdSphere;
            _drydockThresholdSphere.Subscribe(this);
        }

        public void CreateDrydockedItem()
        {
            var sh=new SerializationHelper();
            if(sh.MightBeJson(input.text))
            {
                var command = sh.DeserializeFromJsonString<TokenCreationCommand>(input.text);
                command.Execute(Context.Unknown(),_drydockThresholdSphere);
            }
            else
            {
                var elementId = input.text;
                var element = Watchman.Get<Compendium>().GetEntityById<Element>(elementId);

                if (element.Id == NullElement.Create().Id)
                    return;

                Context debugContext = new Context(Context.ActionSource.Debug);

                _drydockThresholdSphere.ModifyElementQuantity(elementId, 1, debugContext);

                EncaustDrydockedItem(_drydockThresholdSphere.GetTokenInSlot(), input);
            }
        }

        public void DestroyDrydockedItem()
        {
             _drydockThresholdSphere.GetTokenInSlot().Retire(RetirementVFX.CardTakenShadow);
        }

        public void Mutate()
        {
            var elementToken = _drydockThresholdSphere.GetElementTokenInSlot();
            elementToken.Payload.SetMutation(input.text, 1,true);
        }

        public void Unmutate()
        {
            var elementToken = _drydockThresholdSphere.GetElementTokenInSlot();
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
