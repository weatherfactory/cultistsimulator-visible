#pragma warning disable 0649

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using UnityEngine;
using SecretHistories.Services;
using SecretHistories.UI.Scripts;
using SecretHistories.Constants;

namespace SecretHistories.UI.SlotsContainers {
    public class StartingSlotsManager : MonoBehaviour {

        [SerializeField] ThresholdsWrangler _gridWrangler;
        public CanvasGroupFader canvasGroupFader;
        protected List<ThresholdSphere> validSlots;

        protected ThresholdSphere primarySlot;
        private IVerb _verb;
        private SituationWindow _window;
        private SituationPath _situationPath;




        public IEnumerable<ElementStack> GetStacksInSlots()
        {
            IList<ElementStack> stacks = new List<ElementStack>();
            ElementStack stack;

            foreach (ThresholdSphere slot in GetAllSlots())
            {
                stack = slot.GetElementTokenInSlot().ElementStack;

                if (stack != null)
                    stacks.Add(stack);
            }

            return stacks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="includeElementAspects">true to return aspects for the elements themselves as well; false to include only their aspects</param>
        /// <returns></returns>
        public AspectsDictionary GetAspectsFromSlottedCards(bool includeElementAspects)
        {
            AspectsDictionary currentAspects = new AspectsDictionary();
            ElementStack stack;

            foreach (ThresholdSphere slot in GetAllSlots())
            {
                stack = slot.GetElementTokenInSlot().ElementStack;

                if (stack != null)
                    currentAspects.CombineAspects(stack.GetAspects(includeElementAspects));
            }

            return currentAspects;
        }

        public virtual IList<ThresholdSphere> GetAllSlots()
        {

            return validSlots;
        }

        public ThresholdSphere GetSlotBySaveLocationInfoPath(string saveLocationInfoPath)
        {
            var candidateSlots = GetAllSlots();
            ThresholdSphere slotToReturn = candidateSlots.SingleOrDefault(s => s.GetPath().ToString() == saveLocationInfoPath);
            return slotToReturn;
        }


        //public void Initialise(IVerb verb,SituationWindow window,SituationPath situationPath) {
            
            
        //    var children = GetComponentsInChildren<ThresholdSphere>();
        //    var allSlots = new List<ThresholdSphere>(children);
        //    validSlots = new List<ThresholdSphere>(allSlots.Where(rs => rs.Defunct == false && rs.GoverningSphereSpec != null));

        //    _verb = verb;
        //    _window= window;
        //    _situationPath = situationPath;
            

        //var primarySlotSpecification = verb.Slot;
        //    if(primarySlotSpecification!=null)
        //        primarySlot = BuildSlot(primarySlotSpecification.Label, primarySlotSpecification, null,false);
        //    else
        //        primarySlot = BuildSlot("Primary recipe slot", new SphereSpec(), null);


        //    var otherslots = verb.Slots;
        //    if(otherslots!=null)
        //        foreach (var s in otherslots)
        //            BuildSlot(s.Label, s, null);;

        //}

        public void UpdateDisplay(Situation situation)
        {

            if(situation.CurrentState.IsActiveInThisState(primarySlot))
                    canvasGroupFader.Show();
            else
                    canvasGroupFader.Hide();
        }

        






        //protected  void ClearAndDestroySlot(ThresholdSphere slot, Context context) {
        //    if (slot == null)
        //        return;
        //    if (slot.Defunct)
        //        return;

        //    validSlots.Remove(slot);

        //    // This is all copy & paste from the parent class except for the last line
        //    if (slot.childSlots.Count > 0) {
        //        List<ThresholdSphere> childSlots = new List<ThresholdSphere>(slot.childSlots);
        //        foreach (var cs in childSlots)
        //            ClearAndDestroySlot(cs, context);

        //        slot.childSlots.Clear();
        //    }

        //    //Destroy the slot *before* returning the token to the tabletop
        //    //otherwise, the slot will fire OnCardRemoved again, and we get an infinte loop
        //    _gridWrangler.RemoveThreshold(slot);

        //    if (context != null && context.actionSource == Context.ActionSource.SituationStoreStacks)
        //        return; // Don't return the tokens to tabletop if we

        //    Token tokenContained = slot.GetTokenInSlot();

        //    if (tokenContained != null)
        //        tokenContained.GoAway(context);
        //}

    }

}
