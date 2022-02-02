using System.Collections;
using System.Collections.Generic;
using SecretHistories.Assets.Scripts.Application.Spheres;
using SecretHistories.Entities;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Entities
{
    public class Way : MonoBehaviour
    {

        //Ways are redundant now I'm using AStar, but may become dundant again esp for range.
        public List<Sphere> LinkedSpheres = new List<Sphere>();

        public void Start()
        {
            Watchman.Get<HornedAxe>().RegisterWay(this);
           
        }


        public void LinkSphere(Sphere sphere)
        {
            LinkedSpheres.Add(sphere);
        }
    }
}