using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Fucine;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Elements;
using Assets.TabletopUi.Scripts.Elements.Manifestations;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Services;
using Noon;

namespace Assets.TabletopUi.Scripts.TokenContainers
{
  public class CardsPile: Sphere
  {
      private DeckSpec _deckSpec;

      public override ContainerCategory ContainerCategory => ContainerCategory.Dormant;
      public override bool EnforceUniqueStacksInThisContainer => false;
        public override bool ContentsHidden => true;
        public virtual Type ElementManifestationType => typeof(MinimalManifestation);


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

      public override SpherePath GetPath()
      {
            if(!string.IsNullOrEmpty(_localPath))
                NoonUtility.Log($"We're trying to specify a spherepath ({_localPath}) in cardpile for {_deckSpec?.Id}");
            return new SpherePath(_deckSpec?.Id);
      }


  }
}
