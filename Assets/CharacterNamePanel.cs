using UnityEngine;
using System.Collections;
using TMPro;

public class CharacterNamePanel : MonoBehaviour,ICharacterDetailsDisplay
{
    [SerializeField] private TextMeshProUGUI txtTitle;
    [SerializeField]
    private TextMeshProUGUI txtFirstName;
    [SerializeField]
    private TextMeshProUGUI txtLastName;

    public void UpdateDisplay(Character character)
    {
        txtTitle.text = character.Title;
        txtFirstName.text = character.FirstName;
        txtLastName.text = character.LastName;
    }
}
