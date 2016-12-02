using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Services
{

    public class TabletopObjectBuilder
    {
        private Transform tableLevel;
        string[] legalElementIDs = new string[4] {
            "health",
            "reason",
            "intuition",
            "shilling"
        };

        public TabletopObjectBuilder(Transform tableLevel)
        {
            this.tableLevel = tableLevel;
        }

       public void PopulateTabletop()
        {

            ElementStackToken stack;

            float sTokenHorizSpace = (PrefabFactory.GetPrefab<SituationToken>().transform as RectTransform).rect.width + 20f;
            float sTokenVertiSpace = (PrefabFactory.GetPrefab<SituationToken>().transform as RectTransform).rect.height + 50f;
            float cardWidth = (PrefabFactory.GetPrefab<ElementStackToken>().transform as RectTransform).rect.width + 20f;


            // build verbs
            var verbs = Registry.Compendium.GetAllVerbs();

            for (int i = 0; i < verbs.Count; i++)
            {
                IVerb v = verbs[i];
                SituationCreationCommand command=new SituationCreationCommand(v,null);
                var situationToken=BuildSituation(command);
                
                situationToken.transform.localPosition = new Vector3(-1000f+sTokenHorizSpace, -200f + i * sTokenVertiSpace);
                //replace with locatorinfo
            }


            for (int i = 0; i <= legalElementIDs.GetUpperBound(0); i++)
            {
                stack = PrefabFactory.CreateToken<ElementStackToken>(tableLevel);
                stack.Populate(legalElementIDs[i % legalElementIDs.Length], 3);
                stack.transform.localPosition = new Vector3(-750f + i * cardWidth, 0f);
            }
        }



        public SituationToken BuildSituation(SituationCreationCommand situationCreationCommand, string locatorInfo=null)
        {
            var situationController = new SituationController(Registry.Compendium);

            var newToken = PrefabFactory.CreateToken<SituationToken>(tableLevel,locatorInfo);
            var window = buildSituationWindowForSituationToken(newToken);
            
            situationController.Initialise(situationCreationCommand,newToken,window);

            return newToken;
        }



        private SituationWindow buildSituationWindowForSituationToken(SituationToken situationToken)
        {
            var situationWindow = PrefabFactory.CreateLocally<SituationWindow>(situationToken.transform);

            situationWindow.gameObject.SetActive(false);
            situationWindow.transform.position = situationToken.transform.position;

            return situationWindow;
        }

    }
}
