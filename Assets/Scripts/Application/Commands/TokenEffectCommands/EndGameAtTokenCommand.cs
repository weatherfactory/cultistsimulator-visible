using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.UI;

namespace SecretHistories.Commands
{
    public class EndGameAtTokenCommand: IAffectsTokenCommand
    {
        private Ending Ending;
        private bool HasExecuted=false;

        public EndGameAtTokenCommand(Ending ending)
        {
            Ending = ending;
        }


        public bool ExecuteOn(Token token)
        {
            if(!HasExecuted)
            {
                Watchman.Get<GameGateway>().EndGame(Ending,token);
                HasExecuted = true;
            }
            return true;
        }

        public bool ExecuteOn(ITokenPayload payload)
        {
            return false;
        }
    }
}
