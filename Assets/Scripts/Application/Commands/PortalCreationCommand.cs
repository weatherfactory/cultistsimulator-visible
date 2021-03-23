using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Tokens.TokenPayloads;

namespace SecretHistories.Commands
{
    public class PortalCreationCommand: ITokenPayloadCreationCommand, IEncaustment
    {
        private readonly string _portalId;
        private readonly string _otherworldId;


        public int Quantity => 1;
        public List<PopulateDominionCommand> Dominions { get; set; }

        public PortalCreationCommand(string portalId,string otherworldId)
        {
            _portalId = portalId;
            _otherworldId = otherworldId;
            Dominions = new List<PopulateDominionCommand>();
        }

        public ITokenPayload Execute(Context context)
        {
            var portal=new Portal(_portalId,_otherworldId);
            return portal;
        }
    }
}
