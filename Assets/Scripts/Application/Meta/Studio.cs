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
        [SerializeField] private ThresholdsWrangler elementDrydockWrangler;
        [SerializeField] private InputField input;
        private ThresholdSphere primaryThreshold;

        public void Awake()
        {
            primaryThreshold=elementDrydockWrangler.BuildPrimaryThreshold(new SphereSpec(), SituationPath.Root(), new NullVerb());
        }

        public void CreateDrydockedItem()
        {
            var elementId = input.text;

            
            var tabletopPath =
                new SpherePath(Watchman.Get<Compendium>().GetSingleEntity<Dictum>().DefaultWorldSpherePath);
            var tabletop = Watchman.Get<SphereCatalogue>().GetSphereByPath(tabletopPath);
            var element = Watchman.Get<Compendium>().GetEntityById<Element>(elementId);

            if (element.Id == NullElement.Create().Id)
                return;

            Context debugContext = new Context(Context.ActionSource.Debug);

            primaryThreshold.ModifyElementQuantity(elementId, 1, debugContext);

        }

        public void DestroyDrydockedItem()
        {

            var itemId = input.text;


            primaryThreshold.ModifyElementQuantity(itemId, -1, new Context(Context.ActionSource.Debug));
        }
    }
}
