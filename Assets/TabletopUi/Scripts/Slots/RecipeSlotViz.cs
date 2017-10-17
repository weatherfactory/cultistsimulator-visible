using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.CS.TabletopUI {
    public class RecipeSlotViz : MonoBehaviour {

        [SerializeField] RecipeSlot slot;
        [SerializeField] Animation anim;

        bool isHidden;

        public void TriggerShowAnim() {
            anim.Play("recipe-slot-show");
        }

        void ShowDefault() {
            anim["recipe-slot-hide"].time = 0f;
            anim["recipe-slot-hide"].enabled = true;
            anim["recipe-slot-hide"].weight = 1;
            anim.Sample();
            anim["recipe-slot-hide"].enabled = false;
        }

        public void TriggerHideAnim() {
            isHidden = true;
            slot.Defunct = true;
            anim.Play("recipe-slot-hide");
        }

        private void OnDisable() {
            if (isHidden)
                OnHideEnd();
            else
                ShowDefault();
        }

        public void OnHideEnd() {
            slot.Retire();
        }
    }
 }