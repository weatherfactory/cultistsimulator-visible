using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Spheres;
using SecretHistories.UI;

namespace Assets.Scripts.Application.Spheres
{
    public class NoteNavigationArgs
    {
        public int Index { get; set; }
        public AnimDirection AnimDirection { get; set; }

        public NoteNavigationArgs(int index, AnimDirection animDirection)
        {
            Index = index;
            AnimDirection = animDirection;
        }
    }

    public class NotesSphere: Sphere
    {
        public override SphereCategory SphereCategory => SphereCategory.Notes;
        public override bool AllowStackMerge => false;
        public event Action<NoteNavigationArgs> OnNoteNavigation;

        public override void SetUpWithSphereSpecAndPath(SphereSpec sphereSpec, SpherePath pathForThisThreshold)
        {
            SphereIdentifier = sphereSpec.Id;
            gameObject.name = SphereIdentifier; //this could be more usefully frequent in other sphere implementations
        }

        public int GetNoteIndex()
        {
            string digits= String.Join("", SphereIdentifier.Where(char.IsDigit));


            if (digits.Length > 0)
                return int.Parse(digits);

            throw new ApplicationException($"Can't find index digits for a note sphere in {SphereIdentifier}");
        }

        public override List<SphereSpec> GetChildSpheresSpecsToAddIfThisTokenAdded(Token t, SpheresWrangler s)
        {
            var sphereSpec = new SphereSpec(new NotesSphereSpecIdentifierStrategy(s.GetSpheresCurrentlyWrangledCount()));
            var sphereSpecList=new List<SphereSpec>();

            sphereSpecList.Add(sphereSpec);
            return sphereSpecList;
        }

        //public override void Reveal()
        //{

        //}

        //public override void Hide()
        //{

        //}


        public void ShowPrevPage()
        {
            OnNoteNavigation?.Invoke(new NoteNavigationArgs(GetNoteIndex()-1, AnimDirection.MoveLeft));
        }


       public void ShowNextPage()
        {
            OnNoteNavigation?.Invoke(new NoteNavigationArgs(GetNoteIndex()+1, AnimDirection.MoveRight));
        }


    }
}
