using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Abstract;
using Assets.Scripts.Application.Logic;
using SecretHistories.Elements.Manifestations;

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
      Timeshadow GetTimeshadow();

    }
}
