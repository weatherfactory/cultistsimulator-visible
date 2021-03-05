using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Elements;
using SecretHistories.Constants;
using SecretHistories.Services;


namespace SecretHistories.Spheres
{
    [IsEmulousEncaustable(typeof(Sphere))]
    public class CardPile: Sphere
  {
      private DeckSpec _deckSpec; //can we use ActionId as DeckSpecId? we can, but then we need another way to specify whether it's draw or discard. That could, however, be at Sphere not Spec level.

      public override SphereCategory SphereCategory => SphereCategory.Dormant;
      public override bool EnforceUniqueStacksInThisContainer => false;
        public override bool ContentsHidden => true;
        


        public void SetSpec(DeckSpec deckSpec)
      {
          _deckSpec = deckSpec;
      }
        public void Shuffle(List<string> exceptElements)
      {
          var rnd = new Random();
          var unshuffledList = new List<string>(_deckSpec.Spec);
          var shuffledList = unshuffledList.OrderBy(x => rnd.Next());

            RetireAllTokens();

            foreach (var s in shuffledList)
                if (!exceptElements.Contains(s))
                {
                    var t = new TokenCreationCommand().WithElementStack(s, 1);
                    t.Execute(Context.Unknown(), this);
                }

      }



  }
}
