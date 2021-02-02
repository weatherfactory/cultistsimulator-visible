using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SecretHistories.Abstract;
using SecretHistories.Entities;

namespace Assets.Scripts.Application.Commands
{
    public class DeckInstanceCreationCommand:IEncaustment
    {
        public string Id { get; set; }

        public DeckInstance Execute()
        {
            return null;
        }

        public string ToJson()
        {
            string output = JsonConvert.SerializeObject(this);
            return output;
        }
    }
}
