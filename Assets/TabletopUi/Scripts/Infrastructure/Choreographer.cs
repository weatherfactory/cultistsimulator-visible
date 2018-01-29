using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Services;
using Noon;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    //places, arranges and displays things on the table
    public class Choreographer
    {
        private Tabletop _tabletop;
        private SituationBuilder _situationBuilder;

        public Choreographer(Tabletop tabletop,SituationBuilder situationBuilder, Transform tableLevelTransform, Transform WindowLevelTransform)
        {
            _tabletop = tabletop;
            _situationBuilder = situationBuilder;
        }

        public void ArrangeTokenOnTable(SituationToken token)
        {
            token.transform.localPosition = GetFreeTokenPosition(token, new Vector2(0, -250f));
            _tabletop.DisplaySituationTokenOnTable(token);
        }

        //we place stacks horizontally rather than vertically
        public void ArrangeTokenOnTable(ElementStackToken stack)
        {
            stack.transform.localRotation = Quaternion.identity;
            
            stack.FlipToFaceUp(true);

            _tabletop.GetElementStacksManager().AcceptStack(stack); //moved up here so the localPosition tested for overlaps is relative to tabletop parent. This may all be irrelevant with Martin's rework. - AK


            if (stack.lastTablePos != null) { 
                stack.transform.position = (Vector3) stack.lastTablePos;
            }
            else {
                stack.transform.localPosition = GetFreeTokenPosition(stack, new Vector2(-100f, 0f));
                stack.lastTablePos = stack.transform.position;
            }

            stack.DisplayAtTableLevel(); //without this, the 3D position is in the air, not on the tabletop, after the localPosition change above. This needs fixing, but will presumably become irrelevant with Martin's rework anyway. - AK

        }

        public void BeginNewSituation(SituationCreationCommand scc)
        {
            if (scc.Recipe == null)
                throw new ApplicationException("DON'T PASS AROUND SITUATIONCREATIONCOMMANDS WITH RECIPE NULL");
            //if new situation is beginning with an existing verb: do not action the creation.

            //oh: I could have an scc property which is a MUST CREATE override

           
            var existingSituation = Registry.Retrieve<SituationsCatalogue>().GetRegisteredSituations()
                .SingleOrDefault(sc => sc.situationToken.Id == scc.Recipe.ActionId);
            //grabbing existingtoken: just in case some day I want to, e.g., add additional tokens to an ongoing one rather than silently fail the attempt.

        if (existingSituation != null)
            {
                if (existingSituation.Situation.State == SituationState.Complete)
                {
                    //verb exists already, but it's completed. We don't want to block new temp verbs executing if the old one is complete, because
                    //otherwise there's an exploit to, e.g., leave hazard finished but unresolved to block new ones appearing.
                    //So nothing happens in this branch except logging.
                    NoonUtility.Log("Created duplicate verb, because previous one is complete.");
                }
                else
                { 
                NoonUtility.Log("Tried to create " + scc.Recipe.Id + " for verb " + scc.Recipe.ActionId + " but that verb is already active.");
                //end execution here
                return;
                }
            }
            var token = _situationBuilder.CreateTokenWithAttachedControllerAndSituation(scc);

            //if token has been spawned from an existing token, animate its appearance
            if (scc.SourceToken != null)
            {
                var tokenAnim = token.gameObject.AddComponent<TokenAnimation>();
                tokenAnim.onAnimDone += SituationAnimDone;
                tokenAnim.SetPositions(scc.SourceToken.RectTransform.anchoredPosition3D,
                    Registry.Retrieve<Choreographer>().
                        GetFreeTokenPosition(token, new Vector2(0, -250f)));
                tokenAnim.SetScaling(0f, 1f);
                tokenAnim.StartAnim();
            }
            else
            {
                Registry.Retrieve<Choreographer>().ArrangeTokenOnTable(token);
            }

        }

       public void MoveElementToSituationSlot(ElementStackToken stack, TokenAndSlot tokenSlotPair)
        {
            var stackAnim = stack.gameObject.AddComponent<TokenAnimationToSlot>();
            stackAnim.onElementSlotAnimDone += ElementGreedyAnimDone;
            stackAnim.SetPositions(stack.RectTransform.anchoredPosition3D, tokenSlotPair.Token.GetOngoingSlotPosition());
            stackAnim.SetScaling(1f, 0.35f);
            stackAnim.SetTargetSlot(tokenSlotPair);
            stackAnim.StartAnim(0.2f);
        }

        void ElementGreedyAnimDone(ElementStackToken element, TokenAndSlot tokenSlotPair)
        {
            if (!tokenSlotPair.RecipeSlot.Equals(null))
                tokenSlotPair.RecipeSlot.AcceptStack(element);
        }

        void SituationAnimDone(SituationToken token)
        {
            _tabletop.DisplaySituationTokenOnTable(token);
        }


        private Vector3 GetFreeTokenPosition(DraggableToken token, Vector2 candidateOffset)
        {
            Vector2 marginPixels = new Vector2(50f, 50f);
            Vector2 candidatePos = new Vector2(0f, 250f);

            float arbitraryYCutoffPoint = -1000;

            while (TokenOverlapsPosition(token, marginPixels, candidatePos) && candidatePos.y > arbitraryYCutoffPoint)
                candidatePos += candidateOffset;

            return candidatePos;
        }

        private bool TokenOverlapsPosition(DraggableToken token, Vector2 marginPixels, Vector2 candidatePos)
        {
            foreach (var t in _tabletop.GetTokenTransformWrapper().GetTokens())
            {
                if (token != t
                    && candidatePos.x - t.transform.localPosition.x < marginPixels.x
                    && candidatePos.x - t.transform.localPosition.x > -marginPixels.x
                    && candidatePos.y - t.transform.localPosition.y < marginPixels.y
                    && candidatePos.y - t.transform.localPosition.y > -marginPixels.y)
                {
                    return true;
                }

            }

            return false;
        }
    }
}
