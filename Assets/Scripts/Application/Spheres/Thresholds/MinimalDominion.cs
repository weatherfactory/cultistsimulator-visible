using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Entities;
using SecretHistories.Services;
using SecretHistories.Spheres;
using UnityEngine;

namespace SecretHistories.UI
{
    [IsEncaustableClass(typeof(PopulateDominionCommand))]
    public class MinimalDominion: MonoBehaviour, IDominion
    {
        
        public  List<Sphere> Spheres { get; }=new List<Sphere>();

        public Sphere GetSphereById(string Id)
        {
            return Spheres.SingleOrDefault(s => s.Id == Id);
        }

        public Sphere CreateSphere(SphereSpec spec)
        {
            var newSphere = Watchman.Get<PrefabFactory>().InstantiateSphere(spec);
            Spheres.Add(newSphere);
            return newSphere;
        }
    }
}
