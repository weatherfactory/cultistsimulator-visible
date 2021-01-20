using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Entities;
using SecretHistories.Interfaces;
using SecretHistories.UI;

using UnityEngine;

namespace SecretHistories.Constants
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
            
            var stacks = Watchman.Get<TabletopManager>()._tabletop.GetElementTokens();

            var animatables = new List<Token>();
            
            foreach (var stack in stacks)
                if (!stack.Defunct && stack.CanAnimateArt() && stack. Element.Id != lastAnimID)
                    animatables.Add(stack);

            //List<Token> tokens =new List<Token>(Registry.Get<SituationsCatalogue>().GetAnimatables());

       //     animatables.AddRange(tokens.Where(t => t.EntityId != lastAnimID));



            if (animatables.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, animatables.Count);

                animatables[index].StartArtAnimation();
                lastAnimID = animatables[index].Element.Id;
				numAnimsRemainingToAirSound--;

				if (numAnimsRemainingToAirSound <= 8) {
					SoundManager.PlaySfx("TokenAnimAir");
					SetNextAirSoundCount();
				}
            }
        }

    }
}
