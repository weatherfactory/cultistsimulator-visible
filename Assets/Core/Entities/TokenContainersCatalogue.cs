using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi;

namespace Assets.Core.Entities {
    public interface IStacksChangeSubscriber {
        void NotifyStacksChanged();
    }

    public class TokenContainersCatalogue {

        private readonly HashSet<ITokenContainer> _currentTokenContainers;
        private readonly HashSet<IStacksChangeSubscriber> _subscribers;

        public TokenContainersCatalogue() {
            _currentTokenContainers = new HashSet<ITokenContainer>();
            _subscribers = new HashSet<IStacksChangeSubscriber>();
        }

        public HashSet<ITokenContainer> GetRegisteredTokenContainers() {
            return _currentTokenContainers;
        }

        public void RegisterTokenContainer(ITokenContainer tokenContainer) {
            _currentTokenContainers.Add(tokenContainer);
        }

        public void DeregisterTokenContainer(ITokenContainer tokenContainer) {
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
