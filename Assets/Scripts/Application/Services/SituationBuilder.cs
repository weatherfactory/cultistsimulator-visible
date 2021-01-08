
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using SecretHistories.Constants;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using UnityEngine;

namespace SecretHistories.Services {
    public class SituationBuilder:MonoBehaviour
    {

        [SerializeField] private SituationWindow situationWindowPrefab;



        public Situation CreateSituationWithAnchorAndWindow(SituationCreationCommand command)
        {
            var situation = CreateSituationFromCommand(command);

            var sphereCatalogue = Registry.Get<SphereCatalogue>();
            var anchorSphere = sphereCatalogue.GetSphereByPath(command.AnchorLocation.AtSpherePath);
            var windowSphere = sphereCatalogue.GetSphereByPath(new SpherePath(Registry.Get<Compendium>().GetSingleEntity<Dictum>().DefaultWindowSpherePath));

            var newAnchor = AttachNewAnchor(command.AnchorLocation.Anchored3DPosition, situation, anchorSphere);
            var newWindow=AttachNewWindow(windowSphere, newAnchor, situation);


            if (command.Open)
                situation.OpenAtCurrentLocation();
            else
                situation.Close();


            //if token has been spawned from an existing token, animate its appearance
            if (command.SourceToken == null)
                
            {
                //disabled for now: pass the free position instead of trying to find one after the fact, because this resets intended position
           //     Registry.Get<Choreographer>().ArrangeTokenOnTable(newAnchor, null);
            }
            else
            {
                SoundManager.PlaySfx("SituationTokenCreate");

                var spawnedTravelItinerary=new TokenTravelItinerary(command.SourceToken.TokenRectTransform.anchoredPosition3D,
                    anchorSphere.Choreographer.GetFreeLocalPosition(newAnchor, command.SourceToken.ManifestationRectTransform.anchoredPosition))
                    .WithDuration(1f)
                    .WithSphereRoute(windowSphere,anchorSphere)
                    .WithScaling(0f,1f);

                newAnchor.TravelTo(spawnedTravelItinerary);
            }

            return situation;
        }

        public Situation CreateSituationFromCommand(SituationCreationCommand command)
        {
            Situation newSituation = new Situation(command);
            Registry.Get<SituationsCatalogue>().RegisterSituation(newSituation);
            return newSituation;
        }

        public Token AttachNewAnchor(Vector3 position, Situation situation, Sphere anchorSphere)
        {
            var newAnchor = Registry.Get<PrefabFactory>().CreateLocally<Token>(anchorSphere.transform);
            situation.AttachAnchor(newAnchor);
            anchorSphere.AcceptToken(newAnchor, new Context(Context.ActionSource.Unknown));
            newAnchor.transform.localPosition = position;
            return newAnchor;
        }

        public SituationWindow AttachNewWindow(Sphere windowSphere, Token newAnchor, Situation situation)
        {
            SituationWindow newWindow = Instantiate(situationWindowPrefab);
            newWindow.transform.SetParent(windowSphere.transform);
            newWindow.positioner.Initialise(newAnchor);
            situation.AttachWindow(newWindow);
            return newWindow;
        }
    }
}
