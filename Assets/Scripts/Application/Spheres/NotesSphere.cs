using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Entities;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace Assets.Scripts.Application.Spheres
{
    public class NavigationArgs
    {
        public int Index { get; set; }

        public NavigationAnimationDirection FirstNavigationDirection { get; set; }
        public NavigationAnimationDirection FinalNavigationDirection { get; set; }


        public NavigationArgs(int index, NavigationAnimationDirection firstNavigationDirection,NavigationAnimationDirection finalNavigationDirection)
        {
            Index = index;
            FirstNavigationDirection = firstNavigationDirection;
            FinalNavigationDirection = finalNavigationDirection;
        }
    }
    [IsEmulousEncaustable(typeof(Sphere))]
    public class NotesSphere: Sphere
    {
        public override SphereCategory SphereCategory => SphereCategory.Notes;
        public override bool AllowStackMerge => false;
        public event Action<NavigationArgs> OnNoteNavigation;
        public int Index { get; set; }


        /// <summary>
        ///  //Notes spheres always destroy all their contents. We don't want notes to be evicted on to the desktop
        /// </summary>
        /// <param name="sphereRetirementType"></param>
        public override void Retire(SphereRetirementType sphereRetirementType)
        {
            if (Defunct)
                return;

            Defunct = true;
            AddBlock(new SphereBlock(BlockDirection.Inward, BlockReason.Retiring));
            Watchman.Get<HornedAxe>().DeregisterSphere(this);

            DoRetirement(FinishRetirement, SphereRetirementType.Destructive);

        }


        public override void DisplayAndPositionHere(Token token, Context context)
        {
            token.Manifest();
            token.transform.SetParent(transform, true); //this is the default: specifying for clarity in case I revisit
            token.transform.localRotation = Quaternion.identity;
            token.transform.localScale = Vector3.one;

        }



        public void ShowPrevPage()
        {
            OnNoteNavigation?.Invoke(new NavigationArgs(Index-1, NavigationAnimationDirection.MoveLeft, NavigationAnimationDirection.MoveLeft));
        }


       public void ShowNextPage()
        {
            OnNoteNavigation?.Invoke(new NavigationArgs(Index+1, NavigationAnimationDirection.MoveRight, NavigationAnimationDirection.MoveRight));
        }


    }
}
