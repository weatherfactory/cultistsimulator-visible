using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Services;
using UnityEngine;

namespace SecretHistories.Entities
{
    public class Stable: MonoBehaviour
    {
        private Character _currentCharacter;
        private readonly List<Character> characters=new List<Character>();


  
        public void AddNewCharacterAsProtag(Character newCharacter)
        {
            characters.Add(newCharacter);
            _currentCharacter = newCharacter;
        }
        //in case a character is loaded and we have the existing character already referenced somewhere!
        public Character Protag()
        {
            if (_currentCharacter == null)
            {
                var nullCharacterObj = new GameObject("Null Character");
                var nullCharacter = nullCharacterObj.AddComponent<NullCharacter>();
                AddNewCharacterAsProtag(nullCharacter);
            }

            return _currentCharacter;
        }

    }
}
