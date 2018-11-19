using System.Collections.Generic;
using System.Linq;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using TabletopUi.Scripts.Interfaces;

namespace Assets.Editor
{
    public class SituationSimulator
    {
        private readonly ISituationSimulatorSubscriber _subscriber;
        private readonly ICompendium _compendium;
        private readonly Character _character;

        public SituationSimulator(ISituationSimulatorSubscriber subscriber = null)
        {
            _subscriber = subscriber;

            // Import all the content first
            ContentImporter contentImporter = new ContentImporter();
            _compendium = new Compendium();
            contentImporter.PopulateCompendium(_compendium);
            _character = new Character(_compendium.GetAllLegacies().First());

            // Register all the required services used by the simulator
            Registry registry = new Registry();
            registry.Register<ICompendium>(_compendium);
            registry.Register<Character>(_character);
            registry.Register<SituationsCatalogue>(new SituationsCatalogue());
            registry.Register<StackManagersCatalogue>(new StackManagersCatalogue());
            registry.Register<IDice>(new Dice(new SimulatedRollOverride(subscriber)));
            registry.Register<ITabletopManager>(new SimulatedTabletopManager(subscriber));
            registry.Register<INotifier>(new SimulatedNotifier());
            registry.Register<SimulatedTokenContainer>(new SimulatedTokenContainer("Limbo"));
        }

        public void RunSituation(
            string verbId, string primaryElementId, Dictionary<string, string> additionalElementIds = null)
        {
            RunSituation(
                verbId,
                _compendium.GetElementById(primaryElementId),
                additionalElementIds != null ?
                    additionalElementIds.ToDictionary(p => p.Key, p => _compendium.GetElementById(p.Value)) : null);
        }

        public void RunSituation(
            string verbId, Element primaryElement, Dictionary<string, Element> additionalElements = null)
        {
            // Check that all elements have been provided correctly and convert them to stacks
            if (primaryElement == null)
                throw new SituationSimulatorException("Primary element not found");
            SimulatedElementStack primaryElementStack = new SimulatedElementStack();
            primaryElementStack.Populate(primaryElement, 1, Source.Existing());
            Dictionary<string, SimulatedElementStack> additionalStacks = new Dictionary<string, SimulatedElementStack>();
            if (additionalElements != null)
                foreach (var slot in additionalElements)
                {
                    if (slot.Value == null)
                        throw new SituationSimulatorException("Element not found for slot '" + slot.Key + "'");
                    SimulatedElementStack additionalElementStack = new SimulatedElementStack();
                    additionalElementStack.Populate(slot.Value, 1, Source.Existing());
                    additionalStacks.Add(slot.Key, additionalElementStack);
                }

            // Check that the verb exists, create it otherwise
            IVerb verb = _compendium.GetVerbById(verbId) ?? new CreatedVerb(verbId, "", "");

            // Initialize all the simulation components for this situation
            SituationController controller = new SituationController(_compendium, _character);
            SimulatedSituationDetails details = new SimulatedSituationDetails();
            SimulatedSituationAnchor anchor = new SimulatedSituationAnchor(verb, controller);
            SituationCreationCommand command = new SituationCreationCommand(verb, null, SituationState.Unstarted);
            controller.Initialise(command, anchor, details, null, new SimulatedSituationClock(command.Recipe, controller, _subscriber));
            details.Initialise(verb, controller, null);

            // Add the primary element to the primary slot
            if (!details.TryAddStackToStartingSlot(null, primaryElementStack))
                throw new SituationSimulatorException("Failed to add '" + primaryElement.Id + "' to primary slot");

            // Add the additional elements to the additional slots, if any
            foreach (var slotStack in additionalStacks)
                if (!details.TryAddStackToStartingSlot(slotStack.Key, slotStack.Value))
                    throw new SituationSimulatorException(
                        "Failed to add '" + slotStack.Value.EntityId + "' to '" + slotStack.Key + "' slot");

            // Try to start the recipe, then run it to completion.
            controller.AttemptActivateRecipe();
            while (controller.SituationClock.State != SituationState.Complete)
                controller.ExecuteHeartbeat(0);
        }
    }
}
