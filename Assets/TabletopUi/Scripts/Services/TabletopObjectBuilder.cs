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


        public TabletopObjectBuilder(Transform tableLevel)
        {
            this.tableLevel = tableLevel;
        }

       public void PopulateTabletop()
        {

            float sTokenHorizSpace = (PrefabFactory.GetPrefab<SituationToken>().transform as RectTransform).rect.width + 20f;
            float sTokenVertiSpace = (PrefabFactory.GetPrefab<SituationToken>().transform as RectTransform).rect.height + 50f;
   
            // build verbs
            var verbs = Registry.Retrieve<ICompendium>().GetAllVerbs();

            for (int i = 0; i < verbs.Count; i++)
            {
                IVerb v = verbs[i];
                SituationCreationCommand command=new SituationCreationCommand(v,null);
                var situationToken=BuildSituation(command);
                
                situationToken.transform.localPosition = new Vector3(-700f+sTokenHorizSpace, -200f + i * sTokenVertiSpace);
                //replace with locatorinfo
            }

        }



        public SituationToken BuildSituation(SituationCreationCommand situationCreationCommand, string locatorInfo=null)
        {
            var situationController = new SituationController(Registry.Retrieve<ICompendium>(),Registry.Retrieve<Character>());

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
