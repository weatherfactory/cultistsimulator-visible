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
        private readonly Situation _forSituation;
        private readonly SpherePath _windowSpherePath;

        public WindowCreationCommand(Situation forSituation,SpherePath windowSpherePath)
        {
            _forSituation = forSituation;
            _windowSpherePath = windowSpherePath;
        }

        public SituationWindow Execute(SphereCatalogue sphereCatalogue)
        {
            var sphere = sphereCatalogue.GetSphereByPath(_windowSpherePath);


            var newWindow = Watchman.Get<PrefabFactory>().CreateLocally<SituationWindow>(sphere.transform);
            
            

//            newWindow.transform.SetParent(windowSphere.transform);
            newWindow.positioner.Initialise(_forSituation.GetAnchor());

            _forSituation.Attach(newWindow);

            return newWindow;
        }

    }
}
