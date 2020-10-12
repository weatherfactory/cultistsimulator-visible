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
  public class CardsPile: AbstractTokenContainer
  {
      private DeckSpec _deckSpec;

      public override void Start()
      {
          EnforceUniqueStacksInThisContainer = false;
        base.Start();
      }


        public void SetSpec(DeckSpec deckSpec)
      {
          _deckSpec = deckSpec;
      }

      

        public void Shuffle(List<string> exceptElements)
      {
          var rnd = new Random();
          var unshuffledList = new List<string>(_deckSpec.Spec);
          var shuffledList = unshuffledList.OrderBy(x => rnd.Next());

            RetireAllStacks();

            foreach (var s in shuffledList)
                if(!exceptElements.Contains(s))
                   ProvisionElementStack(s, 1, Source.Fresh(), new Context(Context.ActionSource.Unknown));
      }

      public override string GetSaveLocationForToken(AbstractToken token)
      {

          return _deckSpec?.Id;
      }


  }
}
