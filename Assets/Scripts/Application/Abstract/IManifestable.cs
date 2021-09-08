using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Abstract;
using SecretHistories.Enums;
using SecretHistories.Logic;
using SecretHistories.UI;

namespace SecretHistories.Abstract
{
    public interface IManifestable: IHasAspects
    {
        string EntityId { get; }
        string Label { get; }
      string Description { get; }
      int Quantity { get; }
      string UniquenessGroup { get; }
      bool Unique { get; }
      string Icon { get; }
      string GetIllumination(string key);
      void SetIllumination(string key, string value);
        Timeshadow GetTimeshadow();
        bool RegisterDominion(AbstractDominion dominion);
        public List<AbstractDominion> Dominions { get; }
        public bool Metafictional { get; }
        public bool Retire(RetirementVFX vfx);

    }
}
