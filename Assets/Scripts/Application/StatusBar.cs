#pragma warning disable 0649
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Services;
using SecretHistories.UI;
using TMPro;
using UnityEngine;

public class StatusBar : MonoBehaviour,ICharacterSubscriber
{

    [SerializeField] private TMP_InputField CharacterName;
    [SerializeField] private TextMeshProUGUI CharacterProfession;
    [SerializeField] private ElementOverview _elementOverview;

    public void Awake()
    {
        var w=new Watchman();
        w.Register(this);
    }

    public void AttachToCharacter(Character character)
    {
        character.Subscribe(this);
        CharacterName.text = character.Name;
        CharacterProfession.text = character.Profession;
        Watchman.Get<HornedAxe>().Subscribe(_elementOverview);
        _elementOverview.SetDisplayElementsForLegacy(character.ActiveLegacy);
    }

    public void ChangeCharacterName(string newName)
    {
        Character currentCharacter = Watchman.Get<Stable>().Protag();

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
