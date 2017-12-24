using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;

namespace Assets.Core.Entities
{
    public class StackManagersCatalogue
    {
        private readonly List<IElementStacksManager> _currentElementStackManagers;

        public StackManagersCatalogue()
        {
            _currentElementStackManagers = new List<IElementStacksManager>();
        }

        public List<IElementStacksManager> GetRegisteredStackManagers()
        {
            return _currentElementStackManagers.ToList();
        }

        public void RegisterStackManager(IElementStacksManager stackManager)
        {
            _currentElementStackManagers.Add(stackManager);
        }

        public void DeregisterStackManager(IElementStacksManager stackManager)
        {
            _currentElementStackManagers.Remove(stackManager);
        }

    }
}
