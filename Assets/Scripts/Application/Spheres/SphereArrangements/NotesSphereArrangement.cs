using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Enums;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Assets.Scripts.Application.Spheres.SphereArrangements
{
    public class NotesSphereArrangement: AbstractSphereArrangement
    {
        private List<Sphere> _arrangingSpheres=new List<Sphere>();

        public override void AddNewSphereToArrangement(Sphere newSphere, int index)
        {
            
            
            newSphere.transform.localPosition = Vector3.zero;
    newSphere.GetRectTransform().SetParent(this.transform);
    newSphere.GetRectTransform().anchoredPosition = Vector2.zero;
    newSphere.GetRectTransform().sizeDelta=new Vector2(0,0);


    //hide all but the last *existing* sphere. The just-added sphere is an empty child sphere
    foreach (var s in _arrangingSpheres)
    {
        if(_arrangingSpheres.Last()==s)
            s.Reveal();
        else
            s.Shroud();
    }

    //now add, and hide, the new sphere
            _arrangingSpheres.Add(newSphere);
            (newSphere as NotesSphere).OnNoteNavigation += RespondToNoteNavigation;
            newSphere.Shroud();

        }

        public void RespondToNoteNavigation(NoteNavigationArgs args)
        {
            var indexToShow = args.Index;
            
            if (indexToShow + 1 > _arrangingSpheres.Count)
            {
                NoonUtility.Log(
                    $"Trying to show notes sphere {indexToShow}, but there are only {_arrangingSpheres.Count} spheres in the list under consideration.");
                return;
            }

            foreach (var s in _arrangingSpheres)
            {
                int thisIndex = _arrangingSpheres.IndexOf(s);
                if(thisIndex==indexToShow)
                    _arrangingSpheres[indexToShow].Reveal();
                else
                    s.Shroud();
            }

            SoundManager.PlaySfx("SituationWindowTextMove");
        }
    }
}
