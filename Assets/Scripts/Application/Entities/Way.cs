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
        public List<Sphere> LinkedSpheres = new List<Sphere>();

        public void Start()
        {
            Watchman.Get<HornedAxe>().RegisterWay(this);
            //TODO: hide the Way on startup unless debugging.
        }


        public void LinkSphere(Sphere sphere)
        {
            LinkedSpheres.Add(sphere);
        }
    }
}