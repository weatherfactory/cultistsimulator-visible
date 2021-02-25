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
  public interface IDominion

  {
      List<Sphere> Spheres { get; }
      Sphere CreateSphere(SphereSpec spec);
      Sphere GetSphereById(string id);
  }
}
