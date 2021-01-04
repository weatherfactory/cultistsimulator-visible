#pragma warning disable 0649
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SecretHistories.Interfaces;
using SecretHistories.Services;
using SecretHistories.UI;
using SecretHistories.Interfaces;
using TMPro;
using UnityEngine;

public class StatusBar : MonoBehaviour,ICharacterSubscriber
{

    [SerializeField] private TMP_InputField CharacterName;
    [SerializeField] private TextMeshProUGUI CharacterProfession;

    public void Start()
    {
        Registry.Get<Character>().Subscribe(this);
    }

    public void ChangeCharacterName(string newName)
    {
        Character currentCharacter = Registry.Get<Character>();

        if(currentCharacter.Name!=newName)
            currentCharacter.Name = newName;
        
    }


    public void CharacterNameUpdated(string newName)
    {
        CharacterName.text = newName;
    }

    public void CharacterProfessionUpdated(string newProfession)
    {
        CharacterProfession.text = newProfession;
    }
}
