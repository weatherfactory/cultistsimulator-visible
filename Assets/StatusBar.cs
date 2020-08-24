#pragma warning disable 0649
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Assets.Core.Interfaces;
using Assets.Core.Services;
using Assets.CS.TabletopUI;
using TMPro;
using UnityEngine;

public class StatusBar : MonoBehaviour
{

    [SerializeField] private TMP_InputField CharacterName;
    [SerializeField] private TextMeshProUGUI CharacterProfession;


    public void ChangeCharacterName(string newName)
    {
        Character currentCharacter = Registry.Get<Character>();
        
        Chronicler chronicler = Registry.Get<Chronicler>();
        if(currentCharacter.Name!=newName)
        { 
            chronicler.CharacterNameChanged(newName);
            currentCharacter.Name = newName;
        }
    }

    public void UpdateCharacterDetailsView(Character storage)
    {
        CharacterName.text = storage.Name;
        CharacterProfession.text = storage.Profession;
    }



}
