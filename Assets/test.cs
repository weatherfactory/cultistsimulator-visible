using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class test : MonoBehaviour
{
    private int counter;
    // Use this for initialization
    void Start()
    {
        counter = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.anyKeyDown)
        { 
            counter++;
        gameObject.GetComponent<Text>().text += ("\n" + counter.ToString());
        }
    }
}
