﻿using System;
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
    public class TokenTransformWrapper : ITokenTransformWrapper {

        protected Transform wrappedTransform;
        protected ITokenContainer wrappedContainer; //one container can have multiple wrapped transforms

        public TokenTransformWrapper(Transform t) {
            wrappedTransform = t;
            wrappedContainer = wrappedTransform.GetComponent<ITokenContainer>();
            Assert.IsNotNull(wrappedContainer, "not a container!");
        }

        public IElementStack ProvisionElementStack(string elementId, int quantity, Source stackSource, string locatorid = null) {
            IElementStack stack = PrefabFactory.CreateToken<ElementStackToken>(wrappedTransform, locatorid);
            stack.Populate(elementId, quantity,stackSource);
            Accept(stack);
            return stack;
        }

        public ElementStackToken ProvisionElementStackAsToken(string elementId, int quantity, Source stackSource, string locatorid = null) {
            return ProvisionElementStack(elementId, quantity,Source.Existing(), locatorid) as ElementStackToken;
        }

        public virtual void Accept(IElementStack stack) {
            Accept(stack as DraggableToken);
        }

        public virtual void Accept(DraggableToken token) {
            token.transform.SetParent(wrappedTransform);
            token.transform.localPosition = Vector3.zero;
            token.transform.localRotation = Quaternion.identity;
            token.transform.localScale = Vector3.one;
            token.SetContainer(wrappedContainer);
        }

        public virtual IEnumerable<DraggableToken> GetTokens() {
            return wrappedTransform.GetComponentsInChildren<DraggableToken>();
        }

        public virtual IEnumerable<SituationToken> GetSituationTokens() {
            return wrappedTransform.GetComponentsInChildren<SituationToken>();
        }

        public virtual IEnumerable<IElementStack> GetStacks() {
            // we only want ElementStacks that are the children of the wrapped transform, not of any grandchildren and onwards
            var allCandidateStacks = wrappedTransform.GetComponentsInChildren<ElementStackToken>();
            List<IElementStack> firstLevelChildren = new List<IElementStack>();
            foreach (var s in allCandidateStacks) {
                if (s.transform.parent == wrappedTransform)
                    firstLevelChildren.Add(s);
            }

            return firstLevelChildren;
        }
    }
}
