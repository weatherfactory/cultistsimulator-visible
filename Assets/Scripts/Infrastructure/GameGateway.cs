using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using UnityEngine;

namespace Assets.Scripts.Infrastructure
{
    public class GameGateway:MonoBehaviour
    {

        public void Start()
        {
            var r=new Registry();
            r.Register(this);
        }

        private void ProvisionStartingVerb(Legacy activeLegacy, Sphere inSphere)
        {
            IVerb v = Registry.Get<Compendium>().GetEntityById<BasicVerb>(activeLegacy.StartingVerbId);

            SituationCreationCommand command = new SituationCreationCommand(v, NullRecipe.Create(), StateEnum.Unstarted,
                new TokenLocation(0f, 0f, -100f, inSphere.GetPath()));

            var situation = Registry.Get<SituationBuilder>().CreateSituationWithAnchorAndWindow(command);

            situation.ExecuteHeartbeat(0f);

        }

        private void ProvisionStartingElements(Legacy activeLegacy,Sphere inSphere)
        {
            AspectsDictionary startingElements = new AspectsDictionary();
            startingElements.CombineAspects(activeLegacy.Effects);  //note: we don't reset the chosen legacy. We assume it remains the same until someone dies again.

            foreach (var e in startingElements)
            {
                var context = new Context(Context.ActionSource.Loading);

                Token token = inSphere.ProvisionElementStackToken(e.Key, e.Value, Source.Existing(), context, Element.EmptyMutationsDictionary());
                inSphere.Choreographer.PlaceTokenAtFreePosition(token, context);
            }
        }


        public void BeginNewGame()
        {
            SetupNewBoard();
            var populatedCharacter =
                Registry.Get<Character>(); //should just have been set above, but let's keep this clean
            populatedCharacter.Reset(populatedCharacter.ActiveLegacy, null);
            Registry.Get<Compendium>().SupplyLevers(populatedCharacter);
            Registry.Get<StageHand>().ClearRestartingGameFlag();
        }


        public void SetupNewBoard()
        {

            Character character = Registry.Get<Character>();
            Sphere tabletopSphere = Registry.Get<SphereCatalogue>().GetDefaultWorldSphere();


            ProvisionStartingVerb(character.ActiveLegacy,tabletopSphere);
            ProvisionStartingElements(character.ActiveLegacy, tabletopSphere);

            Registry.Get<Concursum>().ShowNotification(new NotificationArgs(  character.ActiveLegacy.Label, character.ActiveLegacy.StartDescription));
        }


        public async void LeaveGame()
        {
            Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 3, GameSpeed = GameSpeed.Paused, WithSFX = false });


            ITableSaveState tableSaveState = new TableSaveState(Registry.Get<SphereCatalogue>().GetSpheresOfCategory(SphereCategory.World).SelectMany(sphere => sphere.GetAllTokens())

                , Registry.Get<SituationsCatalogue>().GetRegisteredSituations(), Registry.Get<MetaInfo>());

            var saveTask = Registry.Get<GameSaveManager>()
                .SaveActiveGameAsync(tableSaveState, Registry.Get<Character>(), SourceForGameState.DefaultSave);

            var success = await saveTask;


            if (success)
            {
                Registry.Get<StageHand>().MenuScreen();
            }
            else
            {
                // Save failed, need to let player know there's an issue
                // Autosave would wait and retry in a few seconds, but player is expecting results NOW.
                Registry.Get<LocalNexus>().ToggleOptionsEvent.Invoke();
                GameSaveManager.ShowSaveError();
            }
        }
    }
}
