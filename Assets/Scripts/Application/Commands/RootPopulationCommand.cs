using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Core;
using SecretHistories.Spheres;

namespace SecretHistories.Assets.Scripts.Application.Commands
{
    public class RootPopulationCommand: IEncaustment
    {
        public string Id { get; set; }
        public Dictionary<string, int> Mutations { get; set; }
        public List<Sphere> Spheres { get; set; }
    }
}
