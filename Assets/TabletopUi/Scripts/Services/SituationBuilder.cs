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
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Services {
    public class SituationBuilder {

        private TokenContainer anchorLevel;
        private TokenContainer windowLevel;

        public SituationBuilder(TokenContainer anchorLevel, TokenContainer windowLevel) {
            this.anchorLevel = anchorLevel;
            this.windowLevel = windowLevel;
        }


        public Situation CreateSituation(SituationCreationCommand command)
        {
            Situation situation = new Situation(command);
            Registry.Get<SituationsCatalogue>().RegisterSituation(situation);

            var newAnchor = anchorLevel.ProvisionSituationAnchor(command);
            situation.AttachAnchor(newAnchor);
            
            var newWindow = windowLevel.ProvisionSituationWindow(newAnchor);
            situation.AttachWindow(newWindow);

            if(command.Open)
                situation.OpenAtCurrentLocation();


            //if token has been spawned from an existing token, animate its appearance
            if (command.SourceToken != null)
            {
                newAnchor.AnimateTo(
                     1f,
                     command.SourceToken.RectTransform.anchoredPosition3D,
                     Registry.Get<Choreographer>().GetFreePosWithDebug(newAnchor, command.SourceToken.RectTransform.anchoredPosition, 3),
                     null,
                     0f,
                     1f);
            }
            else
            {
                Registry.Get<Choreographer>().ArrangeTokenOnTable(newAnchor, null);
            }



            if (command.SourceToken != null)
                SoundManager.PlaySfx("SituationTokenCreate");

            situation.ExecuteHeartbeat(0f);

            return situation;
        }

    }
}
