using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Logic;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Elements;
using SecretHistories.Constants;
using SecretHistories.Infrastructure;
using SecretHistories.Manifestations;
using SecretHistories.Services;


namespace SecretHistories.Spheres
{
    [IsEmulousEncaustable(typeof(Sphere))]
    public class DrawPile: Sphere
  {

      public override SphereCategory SphereCategory => SphereCategory.Dormant;
      public override bool EnforceUniqueStacksInThisContainer => false;
      public override bool ContentsHidden => true;
        

        public void ShowContents()
        {
            //Only call this when the deckspec has been chosen from the autocomplete
            //also display in a sensible way positioning-wise

            var dealer = new Dealer(Watchman.Get<DealersTable>());

            if (GetTotalStacksCount() == 0)
                dealer.Shuffle(GoverningSphereSpec.ActionId);


            foreach (var t in _tokens)
                t.ManifestAs(typeof(CardManifestation));
        }
        public void HideContents()
        {
    //
        }


    }
}
