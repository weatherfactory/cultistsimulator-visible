using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Spheres;
using OrbCreationExtensions;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Constants;

namespace Assets.Scripts.Application.Infrastructure.SimpleJsonGameDataImport
{
    public class SimpleJsonSlotImporter
    {

        public static List<SphereSpec> ImportSituationOngoingSlotSpecs(Hashtable htSituation, List<SphereSpec> ongoingSlotsForRecipe)
        {
            List<SphereSpec> ongoingSlotSpecs = new List<SphereSpec>();

            

            if (htSituation.ContainsKey(SaveConstants.SAVE_ONGOINGSLOTELEMENTS))
            {
                var htOngoingSlotStacks = htSituation.GetHashtable(SaveConstants.SAVE_ONGOINGSLOTELEMENTS);

                foreach (string slotPath in htOngoingSlotStacks.Keys)
                {
                    var slotId = slotPath.Split(SpherePath.SEPARATOR)[0];
                    var slotSpec = new SphereSpec(new SimpleSphereSpecIdentifierStrategy(slotId));
                    ongoingSlotSpecs.Add(slotSpec);
                }
            }

            else
            {
                //we don't have any elements in ongoing slots - but we might still have an empty slot from the recipe, which isn't tracked in the save
                //so add the slot to the spec anyway
               foreach (var slot in ongoingSlotsForRecipe)
                    ongoingSlotSpecs.Add(slot);
            }

            return ongoingSlotSpecs;
        }

    }
}
