using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;

namespace Assets.Core.Entities {
    public interface IStacksChangeSubscriber {
        void NotifyStacksChanged();
    }

    public class StackManagersCatalogue {

        private readonly List<ElementStacksManager> _currentElementStackManagers;
        private readonly List<IStacksChangeSubscriber> _subscribers;

        public StackManagersCatalogue() {
            _currentElementStackManagers = new List<ElementStacksManager>();
            _subscribers = new List<IStacksChangeSubscriber>();
        }

        public List<ElementStacksManager> GetRegisteredStackManagers() {
            return _currentElementStackManagers.ToList();
        }

        public void RegisterStackManager(ElementStacksManager stackManager) {
            _currentElementStackManagers.Add(stackManager);
        }

        public void DeregisterStackManager(ElementStacksManager stackManager) {
            _currentElementStackManagers.Remove(stackManager);
        }

        
        public void Reset()
        {
        _subscribers.Clear();
                foreach (var sm in GetRegisteredStackManagers())
                {
                    if (!sm.PersistBetweenScenes)
                        _currentElementStackManagers.Remove(sm);
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
