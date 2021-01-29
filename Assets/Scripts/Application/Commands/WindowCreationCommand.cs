using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Services;
using SecretHistories.UI;

namespace Assets.Scripts.Application.Commands
{
   public class WindowCreationCommand
    {
        public TokenLocation Location { get; set; }

        public WindowCreationCommand(TokenLocation location)
        {
            Location = location;
        }

        public SituationWindow Execute(SphereCatalogue sphereCatalogue)
        {
            var sphere = sphereCatalogue.GetSphereByPath(Location.AtSpherePath);
            var newWindow = Watchman.Get<PrefabFactory>().CreateLocally<SituationWindow>(sphere.transform);
            
            return newWindow;
        }

    }
}
