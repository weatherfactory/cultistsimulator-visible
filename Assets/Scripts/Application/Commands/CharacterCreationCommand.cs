using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.Enums;

namespace SecretHistories.Commands
{
   public class CharacterCreationCommand
    {
        public string Name { get; set; }
        public string Profession { get; set; }
        public string ActiveLegacyId { get; set; }
        public string EndingTriggeredId { get; set; }


    }
}
