using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Spheres;
using UnityEngine;

namespace SecretHistories.UI
{

  public interface IDominion: IEncaustable

  {
      public DominionEnum Identifier { get; }
      OnSphereAddedEvent OnSphereAdded { get; }
        OnSphereRemovedEvent OnSphereRemoved { get; }
        void RegisterFor(IManifestable manifestable);
        List<Sphere> Spheres { get; }
      Sphere TryCreateSphere(SphereSpec spec);
      Sphere GetSphereById(string id);
      bool RemoveSphere(string id);
      public void Evoke();
      public void Dismiss();
      bool CanCreateSphere(SphereSpec spec);
  }
}
