using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Services
{
    public class TabletopObjectBuilder
    {
        private Transform tableLevel;
        private Transform windowLevel;
        private ElementStacksGateway allStacksGateway;
        string[] legalElementIDs = new string[7] {
            "health",
            "reason",
            "clique",
            "ordinarylife",
            "suitablepremises",
            "occultscrap",
            "shilling"
        };

        public TabletopObjectBuilder(Transform tableLevel,Transform windowLevel,ElementStacksGateway allStacksGateway)
        {
            this.tableLevel = tableLevel;
            this.windowLevel = windowLevel;
            this.allStacksGateway = allStacksGateway;
        }

       public void PopulateTabletop()
        {

            SituationToken situationToken;
            ElementStack stack;

            float boxWidth = (PrefabFactory.GetPrefab<SituationToken>().transform as RectTransform).rect.width + 20f;
            float boxHeight = (PrefabFactory.GetPrefab<SituationToken>().transform as RectTransform).rect.height + 50f;
            float cardWidth = (PrefabFactory.GetPrefab<ElementStack>().transform as RectTransform).rect.width + 20f;


            // build verbs
            var verbs = Registry.Compendium.GetAllVerbs();

            for (int i = 0; i < verbs.Count; i++)
            {
                situationToken = PrefabFactory.CreateTokenWithSubscribers<SituationToken>(tableLevel);
                situationToken.Initialise(verbs[i], allStacksGateway);
                situationToken.transform.localPosition = new Vector3(-1000f + i * boxWidth, boxHeight);

                var situationWindow=PrefabFactory.CreateLocally<SituationWindow>(windowLevel);
                situationWindow.transform.position = situationToken.transform.position;
                situationWindow.gameObject.SetActive(false);
                situationToken.detailsWindow = situationWindow;
                situationWindow.linkedToken = situationToken;
            }


            for (int i = 0; i < 7; i++)
            {
                stack = PrefabFactory.CreateTokenWithSubscribers<ElementStack>(tableLevel);
                stack.Populate(legalElementIDs[i % legalElementIDs.Length], 3);
                stack.transform.localPosition = new Vector3(-1000f + i * cardWidth, 0f);
            }
        }



    }
}
