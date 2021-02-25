using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Entities;
using SecretHistories.Entities.Verbs;
using SecretHistories.Fucine;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Commands
{
    public class MinimalPayloadCreationCommand : ITokenPayloadCreationCommand, IEncaustment
    {
        public string Id { get; set; }
        public int Quantity { get; set; }
        public List<PopulateDominionCommand> Dominions { get; set; }

        public MinimalPayloadCreationCommand()
        {
            Id = "defaultminimalpayloadid";
        }

        public ITokenPayload Execute(Context context)
        {
            var mp = new MinimalPayload(Id);
            var dominion = new GameObject().AddComponent<MinimalDominion>();
            mp. Dominions.Add(dominion); //this may need rethinking if we use PopulateDominionCommands here
            return mp;
        }


    }
}
