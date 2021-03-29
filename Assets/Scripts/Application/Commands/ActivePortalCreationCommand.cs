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
            
            var otherworldLayer = Watchman.Get<OtherworldLayer>();
            
            if(otherworldLayer!=null)
                otherworldLayer.Attach(newConnectedPortal);
            else
                NoonUtility.LogWarning("Can't find otherworld layer to attach portal to");
            
            return newConnectedPortal;
        }
    }
}
