﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Abstract;
using SecretHistories.Elements.Manifestations;
using SecretHistories.Logic;

namespace SecretHistories.Abstract
{
    public interface IManifestable: IHasAspects
    {

      string Label { get; }
      string Description { get; }
      int Quantity { get; }
      string UniquenessGroup { get; }
      bool Unique { get; }
      string Icon { get; }
      string GetIllumination(string key);
      void SetIllumination(string key, string value);
        Timeshadow GetTimeshadow();

    }
}
