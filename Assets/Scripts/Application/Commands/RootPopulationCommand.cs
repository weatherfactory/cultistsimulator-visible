using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Constants;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Spheres;
using SecretHistories.UI;

namespace SecretHistories.Assets.Scripts.Application.Commands
{
    public class RootPopulationCommand: IEncaustment
    {
        public Dictionary<string, int> Mutations { get; set; }
        public List<SphereCreationCommand> Spheres { get; set; }

        public RootPopulationCommand()
        {
            Mutations=new Dictionary<string, int>();
            Spheres=new List<SphereCreationCommand>();
        }

        public void Execute(Context context)
        {
            var root = FucineRoot.Get();
            foreach(var m in Mutations)
                root.SetMutation(m.Key,m.Value,false);

            foreach(var s in Spheres)
                s.ExecuteOn(root, context);
          
        }

        public static RootPopulationCommand ForLegacy(Legacy startingLegacy)
        {
            var rootCommand=new  RootPopulationCommand();
            var tabletoppath=new FucinePath(Watchman.Get<Compendium>().GetSingleEntity<Dictum>().DefaultWorldSpherePath);
            var tabletopId = tabletoppath.GetEndingPathPart().GetId();
            var tabletopSphereSpec = new SphereSpec(typeof(TabletopSphere), tabletopId);
            var tabletopSphereCreationCommand = new SphereCreationCommand(tabletopSphereSpec);
            tabletopSphereCreationCommand.Tokens.AddRange(startingLegacy.GetTokenCreationCommandsToEnactLegacy());
            rootCommand.Spheres.Add(tabletopSphereCreationCommand);
            return rootCommand;
        }
    }
}
