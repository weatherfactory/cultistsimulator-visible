using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Elements;
using SecretHistories.Elements.Manifestations;
using SecretHistories.Constants;
using SecretHistories.Services;


namespace SecretHistories.Spheres
{
  public class CardsPile: Sphere
  {
      private DeckSpec _deckSpec;

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
                if(!exceptElements.Contains(s))
                   ProvisionElementStackToken(s, 1, new Context(Context.ActionSource.Unknown),Element.EmptyMutationsDictionary());
      }

      public override SpherePath GetPath()
      {
            if(!string.IsNullOrEmpty(PathIdentifier))
                NoonUtility.Log($"We're trying to specify a spherepath ({PathIdentifier}) in cardpile for {_deckSpec?.Id}");
            return new SpherePath(_deckSpec?.Id);
      }


  }
}
