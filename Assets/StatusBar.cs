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
        
        Chronicler chronicler = Registry.Retrieve<Chronicler>();
        if(currentCharacter.Name!=newName)
        { 
            chronicler.CharacterNameChanged(newName);
            currentCharacter.Name = newName;
        }
    }

    public void UpdateCharacterDetailsView(IGameEntityStorage storage)
    {
        CharacterName.text = storage.Name;
        CharacterProfession.text = storage.Profession;
        RemoteSettings.Completed += TGNameEasterEgg;
    }

    public void TGNameEasterEgg(bool wasUpdatedFromServer, bool settingsChanged, int serverResponse)
    {
        var remoteName = RemoteSettings.GetString(Registry.Retrieve<Character>().Name);
        if(!string.IsNullOrEmpty(remoteName))
        CharacterName.text = remoteName;
    }

}
