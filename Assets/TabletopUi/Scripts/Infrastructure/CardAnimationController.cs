﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Noon;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public class CardAnimationController: MonoBehaviour
    {
        float timeBetweenAnims = 5f;
        float timeBetweenAnimsVariation = 1f;
        private float nextAnimTime;
        private string lastAnimID; // to not animate the same twice. Keep palyer on their toes
        private IElementStacksManager _tabletopStacksManager;
		int numAnimsRemainingToAirSound = 0;

        public void Initialise(IElementStacksManager tabletopStacksManager)
        {
            _tabletopStacksManager = tabletopStacksManager;
            SetNextAnimTime();
			SetNextAirSoundCount();
        }

        void SetNextAnimTime()
        {
            nextAnimTime = Time.time + timeBetweenAnims - timeBetweenAnimsVariation + UnityEngine.Random.value * timeBetweenAnimsVariation * 2f;
        }

		void SetNextAirSoundCount()
		{
			numAnimsRemainingToAirSound = UnityEngine.Random.Range(8, 20);
		}

        public void CheckForCardAnimations()
        {
            try
            {

    
            if (Time.time >= nextAnimTime)
            {
                TriggerArtAnimation();
                SetNextAnimTime();
            }
            }


            catch (Exception e)
            {
               NoonUtility.Log("Problem in checking for card animations: " + e.Message);
            }
        }

        void TriggerArtAnimation()
        {
            
            var stacks = _tabletopStacksManager.GetStacks();

            var animatableStacks = new List<IElementStack>();

            foreach (var stack in stacks)
                if (!stack.Defunct && stack.CanAnimate() && stack.EntityId != lastAnimID)
                    animatableStacks.Add(stack);

            if (animatableStacks.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, animatableStacks.Count);

                animatableStacks[index].StartArtAnimation();
                lastAnimID = animatableStacks[index].EntityId;
				numAnimsRemainingToAirSound--;

				if (numAnimsRemainingToAirSound <= 8) {
					SoundManager.PlaySfx("TokenAnimAir");
					SetNextAirSoundCount();
				}
            }
        }

    }
}
