using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// This is completely dumb at the moment. I was going to make it editable when I added it, but didn't for non-interesting reasons
/// </summary>
public class CharacterNamePanel : MonoBehaviour,ICharacterInfoSubscriber
{
    [SerializeField] private TextMeshProUGUI txtTitle;
    [SerializeField]
    private TextMeshProUGUI txtFirstName;
    [SerializeField]
    private TextMeshProUGUI txtLastName;

    public void ReceiveUpdate(Character character)
    {
        txtTitle.text = character.Title;
        txtFirstName.text = character.FirstName;
        txtLastName.text = character.LastName;
    }
}
