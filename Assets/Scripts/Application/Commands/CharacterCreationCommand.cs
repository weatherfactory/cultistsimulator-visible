using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Enums;

namespace SecretHistories.Commands
{
   public class CharacterCreationCommand
    {
        public CharacterState State { get; set; }
        public string Name { get; set; }
        public string Profession { get; set; }

    }
}
