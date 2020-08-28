using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Interfaces;
using Noon;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public class IntermittentAnimatableController: MonoBehaviour
    {
       public float timeBetweenAnims = 5f;
       public float timeBetweenAnimsVariation = 1f;
        private float nextAnimTime;
        private string lastAnimID; // to not animate the same twice. Keep player on their toes
        int numAnimsRemainingToAirSound = 0;

        public void Start()
        {
            SetNextAnimTime();
			SetNextAirSoundCount();
        }

        public void Update()
        {
            CheckForCardAnimations();
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
            
            var stacks = Registry.Get<TabletopManager>().GetTabletopStacksManager().GetStacks();

            var animatables = new List<IAnimatable>();
            
            foreach (var stack in stacks)
                if (!stack.Defunct && stack.CanAnimate() && stack.EntityId != lastAnimID)
                    animatables.Add(stack as IAnimatable);

            List<IAnimatable> tokens =new List<IAnimatable>(Registry.Get<SituationsCatalogue>().GetAnimatables());

            animatables.AddRange(tokens.Where(t => t.EntityId != lastAnimID));



            if (animatables.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, animatables.Count);

                animatables[index].StartArtAnimation();
                lastAnimID = animatables[index].EntityId;
				numAnimsRemainingToAirSound--;

				if (numAnimsRemainingToAirSound <= 8) {
					SoundManager.PlaySfx("TokenAnimAir");
					SetNextAirSoundCount();
				}
            }
        }

    }
}
