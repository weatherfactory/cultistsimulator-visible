using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Entities;
using SecretHistories.Spheres;
using UnityEngine;

namespace SecretHistories.UI
{

  public interface IDominion: IEncaustable

  {

      OnSphereAddedEvent OnSphereAdded { get; }
        OnSphereRemovedEvent OnSphereRemoved { get; }
        void RegisterFor(IManifestable manifestable);
        List<Sphere> Spheres { get; }
      Sphere CreateSphere(SphereSpec spec);
      Sphere GetSphereById(string id);
  }
}
