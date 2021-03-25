using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Entities;
using SecretHistories.Services;
using SecretHistories.Tokens.TokenPayloads;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Commands
{
    public class ActivePortalCreationCommand: ITokenPayloadCreationCommand, IEncaustment
    {
        private readonly string _portalId;
        private readonly string _otherworldId;


        public int Quantity => 1;
        public List<PopulateDominionCommand> Dominions { get; set; }

        public ActivePortalCreationCommand(string portalId,string otherworldId)
        {
            _portalId = portalId;
            _otherworldId = otherworldId;
            Dominions = new List<PopulateDominionCommand>();
        }

        public ITokenPayload Execute(Context context)
        {

            var portal = Watchman.Get<Compendium>().GetEntityById<Portal>(_portalId);

            var newConnectedPortal=new ActivePortal(portal);

            //This MUST go here, as soon as the payload is created and before tokens or commands are added, because it's here that the payload spheres get attached.
            var windowSphere = newConnectedPortal.GetWindowsSphere();
            var windowLocation =
                new TokenLocation(Vector3.zero, windowSphere.GetAbsolutePath()); //it shouldn't really be zero, but we don't know the real token loc in the current flow

            var newWindow = Watchman.Get<PrefabFactory>().CreateLocally<OtherworldWindow>(windowSphere.transform);
            newWindow.Attach(newConnectedPortal);



            return newConnectedPortal;
        }
    }
}
