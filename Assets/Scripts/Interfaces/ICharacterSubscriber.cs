using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SecretHistories.Interfaces
{
    public interface ICharacterSubscriber
    {
         void CharacterNameUpdated(string newName);
         void CharacterProfessionUpdated(string newProfession);
    }
}
