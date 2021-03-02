using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Entities;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Spheres.Thresholds
{
   public class ShelfDominion: MonoBehaviour,IDominion
    {
        private ITokenPayload _payload;
        public OnSphereAddedEvent OnSphereAdded { get; }
        public OnSphereRemovedEvent OnSphereRemoved { get; }

        private List<Sphere> _spheres;

        public List<Sphere> Spheres =>new List<Sphere>(_spheres);

        public void RegisterFor(ITokenPayload payload)
        {
            _payload = payload;
            payload.RegisterDominion(this);

            
        }
        
        public Sphere CreateSphere(SphereSpec spec)
        {
            return null;
        }

        public Sphere GetSphereById(string id)
        {
            return _spheres.SingleOrDefault(s => s.Id == id);
        }
    }
}
