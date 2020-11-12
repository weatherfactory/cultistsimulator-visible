using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.Core.Enums;
using Assets.Core.Fucine;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Services {
    public class SituationBuilder:MonoBehaviour
    {

        [SerializeField] private SituationWindow situationWindowPrefab;


        public Situation CreateSituation(SituationCreationCommand command)
        {
            Situation situation = new Situation(command);
            Registry.Get<SituationsCatalogue>().RegisterSituation(situation);

            var sphereCatalogue = Registry.Get<SphereCatalogue>();

            var windowSphere = sphereCatalogue.GetContainerByPath(new SpherePath(Registry.Get<ICompendium>().GetSingleEntity<Dictum>().DefaultWindowSpherePath));

            
            var anchorSphere = sphereCatalogue.GetContainerByPath(command.AnchorLocation.AtSpherePath);

            var newAnchor = Registry.Get<PrefabFactory>().Create<VerbAnchor>();
            anchorSphere.AcceptAnchor(newAnchor, new Context(Context.ActionSource.Unknown));
            newAnchor.transform.localPosition = command.AnchorLocation.Position;
            situation.AttachAnchor(newAnchor);

            var newWindow = Instantiate(situationWindowPrefab);
            newWindow.transform.SetParent(windowSphere.transform);
            newWindow.positioner.Initialise(newAnchor);
            situation.AttachWindow(newWindow,command);


            if (command.Open)
                situation.OpenAtCurrentLocation();
            else
                situation.Close();


            //if token has been spawned from an existing token, animate its appearance
            if (command.SourceAnchor == null)
                
            {
                //disabled for now: pass the free position instead of trying to find one after the fact, because this resets intended position
           //     Registry.Get<Choreographer>().ArrangeTokenOnTable(newAnchor, null);
            }
            else
            {
                SoundManager.PlaySfx("SituationTokenCreate");
                newAnchor.AnimateTo(
                     1f,
                     command.SourceAnchor.RectTransform.anchoredPosition3D,
                     Registry.Get<Choreographer>().GetFreePosWithDebug(newAnchor, command.SourceAnchor.RectTransform.anchoredPosition, 3),
                     null,
                     0f,
                     1f);
            }

            return situation;
        }

    }
}
