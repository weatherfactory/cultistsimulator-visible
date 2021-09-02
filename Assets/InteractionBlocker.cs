using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionBlocker : MonoBehaviour
{
    [SerializeField] private Image image;

    public void Block()
    {
        image.enabled = true;

    }

    public void Unblock()
    {
        image.enabled = false;
    }
}
