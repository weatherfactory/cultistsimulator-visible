using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Infrastructure.SimpleJsonGameDataImport;
using NUnit.Framework;
using SecretHistories.Constants;

namespace Assets.Tests.UnitTests
{
    [TestFixture]
    public class TestsSimpleJsonSlotImporter
    {
        [Test]
        public void ImportsNothingWhenTheresNothing()
        {
            List<SlotSpecification> noRecipeSlots = new List<SlotSpecification>();
            var importedSlotSpecs =
                SimpleJsonSlotImporter.ImportSituationOngoingSlotSpecs(new Hashtable(), noRecipeSlots);
            Assert.AreEqual(0, importedSlotSpecs.Count);

        }

        [Test]
        public void rImportsSingleSlotSpecForRecipe()
        {
            List<SlotSpecification> oneSlotList = new List<SlotSpecification> {new SlotSpecification("oneSlot")};
            var importedSlotSpecs =
                SimpleJsonSlotImporter.ImportSituationOngoingSlotSpecs(new Hashtable(), oneSlotList);
            Assert.AreEqual(1, importedSlotSpecs.Count);
        }

        [Test]
        public void ImportsGreedySlotSpecFromRecipe()
        {
            List<SlotSpecification> oneSlotList = new List<SlotSpecification>
                {new SlotSpecification("greedySlot") {Greedy = true}};
            var importedSlotSpecs =
                SimpleJsonSlotImporter.ImportSituationOngoingSlotSpecs(new Hashtable(), oneSlotList);
            Assert.AreEqual(true, importedSlotSpecs.Single().Greedy);
        }

        [Test]
        public void ImportsRequirementsForSlottSpecFromRecipe()
        {
            List<SlotSpecification> oneSlotList = new List<SlotSpecification>
                {new SlotSpecification("greedySlot") {Greedy = true}};
            var importedSlotSpecs =
                SimpleJsonSlotImporter.ImportSituationOngoingSlotSpecs(new Hashtable(), oneSlotList);
            Assert.AreEqual(true, importedSlotSpecs.Single().Greedy);
        }

        [Test]
        public void InfersSlotFromPresenceOfElementStack()
        {

            Hashtable htWithElemenStack = new Hashtable
            {
                {
                    SaveConstants.SAVE_ONGOINGSLOTELEMENTS, new Hashtable
                    {
                        {"SlotSpecIdThatWeSaved_cf2a73d2-2d2d-4ae5-bc02-bc5ea51dfb00", ""}
                    }
                }
            };

            var importedSlotSpecs =
                SimpleJsonSlotImporter.ImportSituationOngoingSlotSpecs(htWithElemenStack,
                    new List<SlotSpecification>());
            
            Assert.AreEqual("SlotSpecIdThatWeSaved", importedSlotSpecs.Single().Id);
        }
    }

}
