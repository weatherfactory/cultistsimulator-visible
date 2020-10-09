using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;

namespace Assets.TabletopUi.Scripts.TokenContainers
{
    public class DeckTokenContainer:AbstractTokenContainer
    {
        private DeckSpec _deckSpec;

        public void PopulateWithDeckSpec(DeckSpec deckSpec)
        {

        }

        public override string GetSaveLocationForToken(AbstractToken token)
        {
            return $"{nameof(GetType)}_{ _deckSpec.Id}";
        }
    }
}
