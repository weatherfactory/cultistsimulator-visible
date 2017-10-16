using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.CS.TabletopUI {
    public class RecipeSlotViz : MonoBehaviour {

        [SerializeField] RecipeSlot slot;
        [SerializeField] Animation anim;

        public void TriggerShowAnim() {
            anim.Play("recipe-slot-show");
        }

        public void TriggerHideAnim() {
            slot.Defunct = true;
            anim.Play("recipe-slot-hide");
        }

        public void OnHideEnd() {
            slot.Retire();
        }
    }
 }