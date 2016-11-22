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
        private Transform windowLevel;
        string[] legalElementIDs = new string[7] {
            "health",
            "reason",
            "intuition",
            "ordinarylife",
            "suitablepremises",
            "occultscrap",
            "atlasnotes"
        };

        public TabletopObjectBuilder(Transform tableLevel,Transform windowLevel)
        {
            this.tableLevel = tableLevel;
            this.windowLevel = windowLevel;
        }

       public void PopulateTabletop()
        {

            SituationToken situationToken;
            ElementStack stack;

            float sTokenHorizSpace = (PrefabFactory.GetPrefab<SituationToken>().transform as RectTransform).rect.width + 20f;
            float sTokenVertiSpace = (PrefabFactory.GetPrefab<SituationToken>().transform as RectTransform).rect.height + 50f;
            float cardWidth = (PrefabFactory.GetPrefab<ElementStack>().transform as RectTransform).rect.width + 20f;


            // build verbs
            var verbs = Registry.Compendium.GetAllVerbs();

            for (int i = 0; i < verbs.Count; i++)
            {
                situationToken = PrefabFactory.CreateToken<SituationToken>(tableLevel);
                var window = buildSituationWindowForSituationToken(situationToken);
                var situationController =new SituationController(situationToken,window);
                situationToken.Initialise(verbs[i], situationController);
                situationToken.transform.localPosition = new Vector3(-1000f+sTokenHorizSpace, -200f + i * sTokenVertiSpace);

                
            }


            for (int i = 0; i < 7; i++)
            {
                stack = PrefabFactory.CreateToken<ElementStack>(tableLevel);
                stack.Populate(legalElementIDs[i % legalElementIDs.Length], 3);
                stack.transform.localPosition = new Vector3(-750f + i * cardWidth, 0f);
            }
        }

        public SituationToken BuildNewTokenRunningRecipe(string recipeId)
        {
            var recipe = Registry.Compendium.GetRecipeById(recipeId);

            var situationToken = PrefabFactory.CreateToken<SituationToken>(tableLevel);

              IVerb v = Registry.Compendium.GetVerbById(recipe.ActionId);

            if (v==null)
                v=new TransientVerb(recipe.ActionId,recipe.Label,recipe.Description);
            var window = buildSituationWindowForSituationToken(situationToken);
            var situationController = new SituationController(situationToken, window);
            situationToken.Initialise(v, situationController);
            

            situationToken.BeginSituation(recipe);

            return situationToken;
        }

        private SituationWindow buildSituationWindowForSituationToken(SituationToken situationToken)
        {
            var situationWindow = PrefabFactory.CreateLocally<SituationWindow>(windowLevel);
            situationWindow.transform.parent = situationToken.transform;
            var tokenPosition = situationToken.transform.position;
            tokenPosition.x = tokenPosition.x + 100;

            situationWindow.transform.position = tokenPosition;
            situationWindow.gameObject.SetActive(false);
            return situationWindow;
        }

    }
}
