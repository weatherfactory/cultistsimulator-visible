using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts.Infrastructure;

namespace Assets.Core.Entities {
    public interface IStacksChangeSubscriber {
        void NotifyStacksChanged();
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
            _currentTokenContainers.Add(tokenContainer);
        }

        public void DeregisterTokenContainer(TokenContainer tokenContainer) {
            _currentTokenContainers.Remove(tokenContainer);
        }

        
        public void Reset()
        {
        _subscribers.Clear();
        _currentTokenContainers.RemoveWhere(tc => !tc.PersistBetweenScenes);
        }


        public void Subscribe(IStacksChangeSubscriber subscriber) {
            if(!_subscribers.Contains(subscriber))
                   _subscribers.Add(subscriber);
        }

        //called whenever a stack quantity is modified, or a stack moves to another StacksManager
        public void NotifyStacksChanged() {
            foreach (var s in _subscribers)
                s.NotifyStacksChanged();
        }


    }
}
