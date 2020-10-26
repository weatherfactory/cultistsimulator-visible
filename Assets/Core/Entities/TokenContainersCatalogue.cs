﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Infrastructure.Events;

namespace Assets.Core.Entities {
    public interface IStacksChangeSubscriber {
        void NotifyStacksChangedForContainer(ContainerStacksChangedArgs args);
    }

    public class TokenContainersCatalogue {

        private readonly HashSet<TokenContainer> _currentTokenContainers;
        private readonly HashSet<IStacksChangeSubscriber> _subscribers;

        public TokenContainersCatalogue() {
            _currentTokenContainers = new HashSet<TokenContainer>();
            _subscribers = new HashSet<IStacksChangeSubscriber>();
        }

        public HashSet<TokenContainer> GetRegisteredTokenContainers() {
            return _currentTokenContainers;
        }

        public void RegisterTokenContainer(TokenContainer tokenContainer) {
            foreach(var s in _subscribers)
                tokenContainer.OnStacksChanged.AddListener(s.NotifyStacksChangedForContainer);
            
            _currentTokenContainers.Add(tokenContainer);
        }

        public void DeregisterTokenContainer(TokenContainer tokenContainer) {
            foreach (var s in _subscribers)
                tokenContainer.OnStacksChanged.RemoveListener(s.NotifyStacksChangedForContainer);
            
            _currentTokenContainers.Remove(tokenContainer);
        }


        public void Reset()
        {
            foreach(var c in _currentTokenContainers)
                if(!c.PersistBetweenScenes)
                    c.OnStacksChanged.RemoveAllListeners();
            _currentTokenContainers.RemoveWhere(tc => !tc.PersistBetweenScenes);
            
            _subscribers.Clear();
        }


        public void Subscribe(IStacksChangeSubscriber subscriber) {
            _subscribers.Add(subscriber);

            foreach(var c in _currentTokenContainers)
                c.OnStacksChanged.AddListener(subscriber.NotifyStacksChangedForContainer);
        }



    }
}
