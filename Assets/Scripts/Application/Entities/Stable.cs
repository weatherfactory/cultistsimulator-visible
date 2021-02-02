using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SecretHistories.Entities
{
    public class Stable: MonoBehaviour
    {
        private Character _currentCharacter;

        private HashSet<Character> characters=new HashSet<Character>();
        public void AddNewCharacterAsProtag(Character newCharacter)
        {
            characters.Add(newCharacter);
            _currentCharacter = newCharacter;
        }
        //in case a character is loaded and we have the existing character already referenced somewhere!
        public Character Protag()
        {
            return _currentCharacter;
        }

    }
}
