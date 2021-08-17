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
        public string EntityId { get; set; }
        
        public string Id { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public List<PopulateDominionCommand> Dominions { get; set; }
        public Dictionary<string,int> Mutations { get; set; }

        //either specified as an id, and retrieved from content, or created on the fly and passed in through the second constructor for storage
        private Portal _portal;
        public IngressCreationCommand()
        {
            Quantity = 1;
        }

        public IngressCreationCommand(string portalId): this()
        {
            EntityId = portalId;
            Dominions = new List<PopulateDominionCommand>();
        }

        public IngressCreationCommand(Portal portal) : this()
        {
            _portal = portal;
            EntityId = portal.Id;
            Dominions = new List<PopulateDominionCommand>();
        }

        public ITokenPayload Execute(Context context)
        {
            if(_portal==null)
                _portal = Watchman.Get<Compendium>().GetEntityById<Portal>(EntityId);

            if (String.IsNullOrEmpty(Id))
                Id = _portal.DefaultUniqueTokenId();
            var ingress=new Ingress(_portal);
            if (!string.IsNullOrEmpty(Label))
                ingress.Label = Label;
            if (!string.IsNullOrEmpty(Description))
                ingress.Description = Description;
            
            if(Mutations!=null)
                foreach (var m in Mutations)
                    ingress.SetMutation(m.Key,m.Value,false);
          
            return ingress;
        }
    }
}
