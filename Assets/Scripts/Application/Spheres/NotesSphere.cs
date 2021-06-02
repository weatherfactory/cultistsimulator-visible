using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Entities;
using SecretHistories.Commands;
using SecretHistories.Constants;
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
        [SerializeField] private NavigationAnimation _navigationAnimation;
        public override SphereCategory SphereCategory => SphereCategory.Notes;
        public override bool AllowStackMerge => false;
        public event Action<NavigationArgs> OnNoteNavigation;
        public List<Token> PagedTokens=new List<Token>();
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

            if(!PagedTokens.Contains(token))
            {
                foreach(var existingToken in PagedTokens)
                    existingToken.MakeInvisible();
                
                PagedTokens.Add(token);
                int newIndex = PagedTokens.FindIndex(t => t == token); //should always be the high index, but maybe we'll want to insert in the middle later
                var navigationArgs=new NavigationArgs(newIndex,NavigationAnimationDirection.MoveRight,NavigationAnimationDirection.MoveRight);
             //   RespondToNoteNavigation(navigationArgs);
            }

            
        }

        public void RespondToNoteNavigation(NavigationArgs args)
        {
            var indexToShow = args.Index;

            if (indexToShow +1 > PagedTokens.Count)
            {
                NoonUtility.Log(
                    $"Trying to show note {indexToShow}, but there are only {PagedTokens.Count} spheres in the list under consideration.");
                return;
            }


            if (indexToShow < 0)
                //index -1 or lower: there ain't no that
                return;

            foreach (var t in Tokens)
                t.MakeInvisible();


            _navigationAnimation.TriggerAnimation(args, null, OnNoteMoveCompleted);

        }

        public void OnNoteMoveCompleted(NavigationArgs args)
        {
            PagedTokens[args.Index].MakeVisible();
        }

        public void ShowPrevPage()
        {
            OnNoteNavigation?.Invoke(new NavigationArgs(Index-1, NavigationAnimationDirection.MoveLeft, NavigationAnimationDirection.MoveLeft));
        }


       public void ShowNextPage()
        {
            OnNoteNavigation?.Invoke(new NavigationArgs(Index+1, NavigationAnimationDirection.MoveRight, NavigationAnimationDirection.MoveRight));
        }



 
        //public override void SphereRemoved(Sphere sphere)
        //{
        //    if (_arrangingSpheres.Contains(sphere))
        //        _arrangingSpheres.Remove(sphere);
        //}


    }

}
