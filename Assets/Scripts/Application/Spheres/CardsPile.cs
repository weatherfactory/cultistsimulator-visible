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
                if (!exceptElements.Contains(s))
                {
                    var newStackCommand=new ElementStackCreationCommand(s,1);
                    ProvisionElementStackToken(newStackCommand, new Context(Context.ActionSource.Unknown));
                }

      }

      public override SpherePath GetPath()
      {
          return new SpherePath(ParentSituation, _deckSpec?.Id);
      }


  }
}
