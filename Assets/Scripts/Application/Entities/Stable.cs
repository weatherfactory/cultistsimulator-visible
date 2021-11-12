using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Enums;
using SecretHistories.Services;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Entities
{
    public class Stable: MonoBehaviour
    {
        private Character _protagonist;


        public IEnumerable<Character> GetAllCharacters()
        {
            return gameObject.GetComponentsInChildren<Character>(true);
        }

        public Character InstantiateCharacterInStable()
        {
            //we may ultimately use this as a proper stable of characters.
            //At the moment I'm having a sticky time working out add new vs add existing character.
            //also I worry about the save file being cluttered up.
            //So for now, we're just destroying characters every time we instantiate a new one.
            foreach(var c in GetAllCharacters())
              GameObject.Destroy(c.gameObject);

            _protagonist= Watchman.Get<PrefabFactory>().CreateLocally<Character>(transform);
            return _protagonist;
        }

        

        public Character Protag()
        {
            return _protagonist;
        }


    }
}
