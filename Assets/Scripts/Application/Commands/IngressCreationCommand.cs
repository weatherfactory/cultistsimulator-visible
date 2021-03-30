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
    public class IngressCreationCommand: ITokenPayloadCreationCommand, IEncaustment
    {
        private readonly string _portalId;
        private readonly string _otherworldId;


        public int Quantity => 1;
        public List<PopulateDominionCommand> Dominions { get; set; }

        public IngressCreationCommand(string portalId,string otherworldId)
        {
            _portalId = portalId;
            _otherworldId = otherworldId;
            Dominions = new List<PopulateDominionCommand>();
        }

        public ITokenPayload Execute(Context context)
        {

            var portal = Watchman.Get<Compendium>().GetEntityById<Portal>(_portalId);
            var ingress=new Ingress(portal);
            
          
            return ingress;
        }
    }
}
