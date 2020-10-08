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

        private readonly List<ITokenContainer> _currentTokenContainers;
        private readonly List<IStacksChangeSubscriber> _subscribers;

        public TokenContainersCatalogue() {
            _currentTokenContainers = new List<ITokenContainer>();
            _subscribers = new List<IStacksChangeSubscriber>();
        }

        public List<ITokenContainer> GetRegisteredTokenContainers() {
            return _currentTokenContainers.ToList();
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
                foreach (var tc in GetRegisteredTokenContainers())
                {
                    if (!tc.PersistBetweenScenes)
                        _currentTokenContainers.Remove(tc);
                }

            
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
