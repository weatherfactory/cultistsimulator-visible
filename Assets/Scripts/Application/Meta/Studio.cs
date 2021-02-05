using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.NullObjects;
using SecretHistories.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Application.Meta
{
    public class Studio: MonoBehaviour
    {
        [SerializeField] private ThresholdsWrangler drydockWrangler;
        [SerializeField] private InputField input;

        public void Awake()
        {
            drydockWrangler.BuildPrimaryThreshold(new SphereSpec(), SituationPath.Root(), new NullVerb());
        }

        public void CreateDrydockedItem()
        {
            var elementId = input.text;

            var tabletopPath =
                new SpherePath(Watchman.Get<Compendium>().GetSingleEntity<Dictum>().DefaultWorldSpherePath);
            var tabletop = Watchman.Get<SphereCatalogue>().GetSphereByPath(tabletopPath);
            var element = Watchman.Get<Compendium>().GetEntityById<Element>(elementId);


            Context debugContext = new Context(Context.ActionSource.Debug);


            tabletop.ModifyElementQuantity(elementId, 1, debugContext);

        }

        public void DestroyDrydockedItem()
        {

            var itemId = input.text;

            var tabletopPath =
                new SpherePath(Watchman.Get<Compendium>().GetSingleEntity<Dictum>().DefaultWorldSpherePath);
            var tabletop = Watchman.Get<SphereCatalogue>().GetSphereByPath(tabletopPath);

            tabletop.ModifyElementQuantity(itemId, -1, new Context(Context.ActionSource.Debug));
        }
    }
}
