using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Spheres;
using SecretHistories.UI;
using Steamworks;

namespace SecretHistories.Assets.Scripts.Application.Commands
{
    public class PopulateTerrainFeatureCommand: IEncaustment
    {
        public string Id { get; set; }
        public List<SphereCreationCommand> Spheres { get; set; } = new List<SphereCreationCommand>();
        public void Execute(Context context,Sphere sphere)
        {
            Watchman.Get<HornedAxe>().FindSingleOrDefaultTokenById("");
        }
    }
}
