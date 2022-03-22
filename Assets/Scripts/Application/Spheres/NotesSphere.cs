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
using SecretHistories.Manifestations;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace Assets.Scripts.Application.Spheres //should be SecretHistories.Sphere. But that'll break save/load until I make save/load less fussy.
{


    [IsEmulousEncaustable(typeof(Sphere))]
    public class NotesSphere : Sphere
    {
        [SerializeField] private NavigationAnimation _navigationAnimation;
        public override SphereCategory SphereCategory => SphereCategory.Notes;
        public override bool AllowStackMerge => false;
        public List<Token> PagedTokens = new List<Token>();
        public int CurrentIndex { get; set; }

        public override Type GetDefaultManifestationType()
        {
            return typeof(TextManifestation);
        }
        /// <summary>
        ///  //Notes spheres always destroy all their contents. We don't want notes to be evicted on to the desktop
        /// </summary>
        /// <param name="sphereRetirementType"></param>
        public override void Retire(SphereRetirementType sphereRetirementType)
        {
            if (Defunct)
                return;

            Defunct = true;
   
            Watchman.Get<HornedAxe>().DeregisterSphere(this);

            DoRetirement(FinishRetirement, SphereRetirementType.Destructive);

        }


        public override void RetireAllTokens()
        {
            base.RetireAllTokens();
            //Notes spheres keep a separate index and list count, so we can page back and forth.
            //When we didn't empty these out too, we could page back through nonexistent notes.
            PagedTokens.Clear();
            CurrentIndex = 0;
        }

        public override void DisplayAndPositionHere(Token token, Context context)
        {
            token.Manifest();
            token.transform.SetParent(transform, true); //this is the default: specifying for clarity in case I revisit
            token.transform.localRotation = Quaternion.identity;
            token.transform.localScale = Vector3.one;

            if (!PagedTokens.Contains(token))
            {
                token.MakeInvisible(); //we'll make it visible again once we've finished navigating
                PagedTokens.Add(token);
                int newIndex =
                    PagedTokens.FindIndex(t =>
                        t == token); //should always be the high index, but maybe we'll want to insert in the middle later
                var navigationArgs = new NavigationArgs(newIndex, NavigationAnimationDirection.MoveRight,
                    NavigationAnimationDirection.MoveRight);


                Navigate(navigationArgs);

            }

        }

        public void Navigate(NavigationArgs args)
        {
            var indexToShow = args.Index;

            if (indexToShow + 1 > PagedTokens.Count)
            {
                NoonUtility.Log(
                    $"Trying to show note {indexToShow}, but there are only {PagedTokens.Count} spheres in the list under consideration.");
                return;
            }


            if (indexToShow < 0)
                //index -1 or lower: there ain't no that
                return;


            args.OnOutComplete = OnNoteOutComplete;
            args.OnInComplete = OnNoteInComplete;
            if (!this._container.IsOpen)
                args.Instant = true;

            if (args.Instant)
            {
                OnNoteOutComplete(args);
                OnNoteInComplete(args);
                
            }
            else
            {
                args.OnOutComplete = OnNoteOutComplete;
                args.OnInComplete = OnNoteInComplete;
                _navigationAnimation.TriggerAnimation(args);
            }

        }

        public void OnNoteOutComplete(NavigationArgs args)
        {
       
            if (CurrentIndex < PagedTokens.Count)
            {
                PagedTokens[CurrentIndex].MakeInvisible();
            }
            else
            {
                NoonUtility.Log($"error 'Gryla' guard+log: we tried to call OnNoteOutComplete with currentindex {CurrentIndex} when there are only {PagedTokens.Count} text tokens in the Notes Sphere here.", 1, VerbosityLevel.Essential);
            }

            

        }

        public void OnNoteInComplete(NavigationArgs args)
        {
            CurrentIndex = args.Index;
            if(CurrentIndex<PagedTokens.Count)
            {
                PagedTokens[CurrentIndex].MakeVisible();
            }
            else
            {
             NoonUtility.Log($"error 'Gryla' guard+log: we tried to call OnNoteInComplete with index {args.Index} when there are only {PagedTokens.Count} text tokens in the Notes Sphere here.",1,VerbosityLevel.Essential);   
            }
        }




        public void PopNoteOut()
        {
            if (PagedTokens.Count < 1)
                return;
            else
            {
                var tokenToPopOut = PagedTokens[CurrentIndex];
                tokenToPopOut.GoAway(new Context(Context.ActionSource.PlayerDump));
            ShowPrevPage();    
            }
            
        }

        public void ShowPrevPage()
        {
            Navigate(new NavigationArgs(CurrentIndex - 1, NavigationAnimationDirection.MoveLeft, NavigationAnimationDirection.MoveLeft));
        }


       public void ShowNextPage()
        {
            Navigate(new NavigationArgs(CurrentIndex+1, NavigationAnimationDirection.MoveRight, NavigationAnimationDirection.MoveRight));
        }



    }

}
