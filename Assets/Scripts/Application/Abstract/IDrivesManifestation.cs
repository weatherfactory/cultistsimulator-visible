using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Logic;
using SecretHistories.Elements.Manifestations;

namespace SecretHistories.Abstract
{
    public interface IDrivesManifestation
    {
      string Label { get; }
      string Description { get; }
      int Quantity { get; }
      Timeshadow GetTimeshadow();
    }
}
