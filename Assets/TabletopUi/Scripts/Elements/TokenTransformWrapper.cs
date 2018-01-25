using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using UnityEngine;
using UnityEngine.Assertions;

namespace Assets.TabletopUi.Scripts {
    /// <summary>
    /// The TokenTransformWrapper has a nice non-Unity interface so the StacksManager can deal with it
    /// but the work it does is to manage the Unity details
    /// </summary>
    public class TokenTransformWrapper : ITokenPhysicalLocation {

        protected Transform wrappedTransform;
        protected IContainsTokensView ContainsTokensView;

        public TokenTransformWrapper(Transform t) {
            wrappedTransform = t;
            ContainsTokensView = wrappedTransform.GetComponent<IContainsTokensView>();
            Assert.IsNotNull(ContainsTokensView, "not a containsTokens!");
        }

        public IElementStack ProvisionElementStack(string elementId, int quantity, Source stackSource, string locatorid = null) {
            IElementStack stack = PrefabFactory.CreateToken<ElementStackToken>(wrappedTransform, locatorid);
            stack.Populate(elementId, quantity,stackSource);
            DisplayHere(stack);
            return stack;
        }

        public ElementStackToken ProvisionElementStackAsToken(string elementId, int quantity, Source stackSource, string locatorid = null) {
            return ProvisionElementStack(elementId, quantity,Source.Existing(), locatorid) as ElementStackToken;
        }

        public virtual void DisplayHere(IElementStack stack) {
            DisplayHere(stack as DraggableToken);
        }

        public virtual void DisplayHere(DraggableToken token) {
            token.transform.SetParent(wrappedTransform);
            token.transform.localPosition = Vector3.zero;
            token.transform.localRotation = Quaternion.identity;
            token.transform.localScale = Vector3.one;
            token.SetViewContainer(ContainsTokensView);
        }

        public virtual IEnumerable<DraggableToken> GetTokens() {
            return wrappedTransform.GetComponentsInChildren<DraggableToken>();
        }

        public virtual IEnumerable<SituationToken> GetSituationTokens() {
            return wrappedTransform.GetComponentsInChildren<SituationToken>();
        }

    }
}
