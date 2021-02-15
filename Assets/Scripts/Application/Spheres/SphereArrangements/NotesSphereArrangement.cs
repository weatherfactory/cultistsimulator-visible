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

        [SerializeField] private NavigationAnimation _navigationAnimation;
        private List<Sphere> _arrangingSpheres=new List<Sphere>();
        
        public override void AddNewSphereToArrangement(Sphere newSphere, int index)
        {

            RectTransform sphereRectTransform = newSphere.GetRectTransform();

            sphereRectTransform.SetParent(this.transform);
            sphereRectTransform.anchoredPosition3D = Vector3.zero;
            sphereRectTransform.sizeDelta =new Vector3(0,0,0);


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

        public void RespondToNoteNavigation(NavigationArgs args)
        {
            var indexToShow = args.Index;
            
            if (indexToShow + 1 > _arrangingSpheres.Count)
            {
                NoonUtility.Log(
                    $"Trying to show notes sphere {indexToShow}, but there are only {_arrangingSpheres.Count} spheres in the list under consideration.");
                return;
            }

            
            if (indexToShow < 0)
                //index -1 or lower: there ain't no that
                return;
            

            if(indexToShow+1==_arrangingSpheres.Count && !_arrangingSpheres[indexToShow].GetAllTokens().Any())
                //we're trying to move to an empty sphere at the end - one that doesn't have a note in yet
                return;

            _navigationAnimation.TriggerAnimation(args,null,OnNoteMoveCompleted);

            foreach (var s in _arrangingSpheres)
            {
                int thisIndex = _arrangingSpheres.IndexOf(s);
                if(thisIndex==indexToShow)
                    _arrangingSpheres[indexToShow].Reveal();
                else
                    s.Shroud();
            }
        }

        public void OnNoteMoveCompleted(NavigationArgs args)
        {
            _arrangingSpheres[args.Index].Reveal();
        }
    }
}
