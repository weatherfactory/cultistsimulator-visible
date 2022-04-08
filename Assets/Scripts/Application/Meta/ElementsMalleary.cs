using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SecretHistories.Commands;
using SecretHistories.Commands.Encausting;
using SecretHistories.Constants.Events;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Events;
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
        [SerializeField] private AutoCompletingInput input;
      [SerializeField]  private DrydockSphere _elementDrydock;

        public void Awake()
        {
            _elementDrydock.Subscribe(this);
        }

        public void CreateDrydockedItem()
        {
            var sh=new SerializationHelper();
            if(sh.MightBeJson(input.text))
            {
                var command = sh.DeserializeFromJsonString<TokenCreationCommand>(input.text);
                command.Execute(Context.Unknown(),_elementDrydock);
                var tokens = _elementDrydock.GetTokens().ToList();
            
                var first=tokens.FirstOrDefault();
                var second = tokens.LastOrDefault();

                if (first != null && second != null)
                {
                    if(first.CanMergeWithToken(second))
                        first.Payload.InteractWithIncoming(second);
                }

                
            }
            else
            {
                CreateElementFromBestGuessOfElementId();
            }
        }

        private void CreateElementFromBestGuessOfElementId()
        {
            var elementId = input.text;
            var element = Watchman.Get<Compendium>().GetEntityById<Element>(elementId);

            if (element.Id == NullElement.Create().Id)
                return;

            Context debugContext = new Context(Context.ActionSource.Debug);

            var existingTokens = _elementDrydock.GetTokens();
            Token mergeableToken = null;
            foreach (var t in existingTokens)
            {
                if (t.IsValidElementStack())
                    if (t.Payload.EntityId == elementId)
                        mergeableToken = t;
            }

            if (mergeableToken != null)
                mergeableToken.Payload.ModifyQuantity(1, debugContext);
            else
                _elementDrydock.ModifyElementQuantity(elementId, 1, debugContext);


            EncaustDrydockedItem(_elementDrydock.GetTokens().FirstOrDefault(), input);
        }

        public void DestroyDrydockedItem()
        {
            if(_elementDrydock.Tokens.Any())
             _elementDrydock.GetTokens().FirstOrDefault().Retire(RetirementVFX.CardTakenShadow);
        }

        public void Mutate()
        {
            var elementToken = _elementDrydock.GetTokens().FirstOrDefault();
            elementToken.Payload.SetMutation(input.text, 1,true);
        }

        public void Unmutate()
        {
            var elementToken = _elementDrydock.GetTokens().FirstOrDefault();
            elementToken.Payload.SetMutation(input.text, -1, true);
        }

        public void OnSphereChanged(SphereChangedArgs args)
        {
            //
        }

        public void OnTokensChangedForSphere(SphereContentsChangedEventArgs args)
        {
//
        }

        public void OnTokenInteractionInSphere(TokenInteractionEventArgs args)
        {
            if (args.Interaction == Interaction.OnDragEnd)
            {
                input.text = args.Payload.EntityId;
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
