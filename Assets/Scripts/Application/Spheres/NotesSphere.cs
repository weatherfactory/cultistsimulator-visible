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


        public int GetNoteIndex()
        {
            string digits= String.Join("", SphereIdentifier.Where(char.IsDigit));


            if (digits.Length > 0)
                return int.Parse(digits);

            throw new ApplicationException($"Can't find index digits for a note sphere in {SphereIdentifier}");
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
            OnNoteNavigation?.Invoke(new NavigationArgs(GetNoteIndex()-1, NavigationAnimationDirection.MoveLeft, NavigationAnimationDirection.MoveLeft));
        }


       public void ShowNextPage()
        {
            OnNoteNavigation?.Invoke(new NavigationArgs(GetNoteIndex()+1, NavigationAnimationDirection.MoveRight, NavigationAnimationDirection.MoveRight));
        }


    }
}
