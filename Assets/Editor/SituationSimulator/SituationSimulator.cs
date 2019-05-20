using System.Collections.Generic;
using System.Linq;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
using TabletopUi.Scripts.Interfaces;

namespace Assets.Editor
{
    public class SituationSimulator
    {
        private const int MaxExecutions = 500;

        private readonly ISituationSimulatorSubscriber _subscriber;
        private readonly ICompendium _compendium;
        private readonly Character _character;

        public SituationSimulator(ISituationSimulatorSubscriber subscriber = null)
        {
            _subscriber = subscriber;

            Registry registry = new Registry();

            // Set language to English for our tests
            LanguageTable.LoadCulture("en");
            
            // Set up an empty mod manager
            registry.Register(new ModManager(false));

            // Import all the content first
            ContentImporter contentImporter = new ContentImporter();
            _compendium = new Compendium();
            contentImporter.PopulateCompendium(_compendium);
            _character = new Character(_compendium.GetAllLegacies().First());
            
            // Initialise all decks
            foreach (var ds in _compendium.GetAllDeckSpecs()) {
                IDeckInstance di = new DeckInstance(ds);
                _character.DeckInstances.Add(di);
                di.Reset();
            }

            // Register all the required services used by the simulator
            registry.Register<ICompendium>(_compendium);
            registry.Register<Character>(_character);
            _compendium.SupplyLevers(_character);
            registry.Register<SituationsCatalogue>(new SituationsCatalogue());
            registry.Register<StackManagersCatalogue>(new StackManagersCatalogue());
            registry.Register<IDice>(new Dice(new SimulatedRollOverride(subscriber)));
            registry.Register<ITabletopManager>(new SimulatedTabletopManager(subscriber));
            registry.Register<INotifier>(new SimulatedNotifier());
            registry.Register<SimulatedTokenContainer>(new SimulatedTokenContainer("Limbo"));
        }

        public void RunSituation(
            string verbId, SimulatedElementStack primaryStack,
            Dictionary<string, SimulatedElementStack> additionalStacks = null)
        {
            // Check that the verb exists, create it otherwise
            IVerb verb = _compendium.GetVerbById(verbId) ?? new CreatedVerb(verbId, "", "");

            // Initialize all the simulation components for this situation
            SituationController controller = new SituationController(_compendium, _character);
            SimulatedSituationDetails details = new SimulatedSituationDetails();
            SimulatedSituationAnchor anchor = new SimulatedSituationAnchor(verb, controller);
            SituationCreationCommand command = new SituationCreationCommand(verb, null, SituationState.Unstarted);
            controller.Initialise(
                command, anchor, details, null, new SimulatedSituationClock(command.Recipe, controller, _subscriber));
            details.Initialise(verb, controller, null);

            // Add the primary element to the primary slot
            if (!details.TryAddStackToStartingSlot(null, primaryStack))
                throw new SituationSimulatorException("Failed to add '" + primaryStack.EntityId + "' to primary slot");

            // Add the additional elements to the additional slots, if any
            if (additionalStacks != null)
                foreach (var slotStack in additionalStacks)
                    if (!details.TryAddStackToStartingSlot(slotStack.Key, slotStack.Value))
                        throw new SituationSimulatorException(
                            "Failed to add '" + slotStack.Value.EntityId + "' to '" + slotStack.Key + "' slot");

            // Try to start the recipe, then run it to completion.
            controller.AttemptActivateRecipe();
            int numExecutions = 0;
            while (controller.SituationClock.State != SituationState.Complete)
            {
                controller.ExecuteHeartbeat(0);
                if (numExecutions++ >= MaxExecutions)
                    throw new SituationSimulatorException("Too many executions, aborting");
            }
        }
    }
}
