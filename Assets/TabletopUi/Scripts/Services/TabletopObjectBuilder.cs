using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            SituationToken situationToken;
            ElementStackToken stack;

            float sTokenHorizSpace = (PrefabFactory.GetPrefab<SituationToken>().transform as RectTransform).rect.width + 20f;
            float sTokenVertiSpace = (PrefabFactory.GetPrefab<SituationToken>().transform as RectTransform).rect.height + 50f;
            float cardWidth = (PrefabFactory.GetPrefab<ElementStackToken>().transform as RectTransform).rect.width + 20f;


            // build verbs
            var verbs = Registry.Compendium.GetAllVerbs();

            for (int i = 0; i < verbs.Count; i++)
            {
                IVerb v = verbs[i];
                situationToken = PrefabFactory.CreateToken<SituationToken>(tableLevel);
                situationToken.transform.localPosition = new Vector3(-1000f+sTokenHorizSpace, -200f + i * sTokenVertiSpace);
                var window = buildSituationWindowForSituationToken(situationToken);
                var situationController = new SituationController(Registry.Compendium);
                situationController.InitialiseToken(situationToken,v);
                situationController.InitialiseWindow(window);
            }


            for (int i = 0; i <= legalElementIDs.GetUpperBound(0); i++)
            {
                stack = PrefabFactory.CreateToken<ElementStackToken>(tableLevel);
                stack.Populate(legalElementIDs[i % legalElementIDs.Length], 3);
                stack.transform.localPosition = new Vector3(-750f + i * cardWidth, 0f);
            }
        }



        public SituationToken BuildNewTokenRunningRecipe(string recipeId, string locatorId=null,IVerb verb=null)
        {

            var recipe = Registry.Compendium.GetRecipeById(recipeId);
            var situationController = new SituationController(Registry.Compendium);
            if (recipe != null)
                situationController.BeginSituation(recipe);


            if (verb==null) //we may have specified a verb, eg if we're rehydrating the situation
              verb = Registry.Compendium.GetVerbById(recipe.ActionId); //if we haven't, get the default verb

            if (verb == null) //no default verb? then this is a transient verb (nb we might also have specified a transient verb)
                verb = new CreatedVerb(recipe.ActionId,recipe.Label,recipe.Description);

            var newToken = PrefabFactory.CreateToken<SituationToken>(tableLevel,locatorId);
            var window = buildSituationWindowForSituationToken(newToken);
            
            situationController.InitialiseToken(newToken, verb);
            situationController.InitialiseWindow(window);

            
            

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
