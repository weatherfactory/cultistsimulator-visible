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
        Character currentCharacter = Registry.Retrieve<Character>();
        currentCharacter.Name = newName;
        Chronicler chronicler = Registry.Retrieve<Chronicler>();
        chronicler.CharacterNameChanged(newName);
    }

    public void UpdateCharacterDetailsView(IGameEntityStorage storage)
    {
        CharacterName.text = storage.Name;
        CharacterProfession.text = storage.Profession;
    }
        
}
