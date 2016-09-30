using UnityEngine;
using System.Collections;
using OrbCreationExtensions;
using UnityEngine.UI;

public class setup : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

        Hashtable ht = ContentManager.Instance.ImportVerbs();
        ArrayList AllVerbs = ht.GetArrayList("verbs");
        Hashtable v = (Hashtable)AllVerbs[0];
        string vDescription = v.GetString("description");


        Hashtable htElements = ContentManager.Instance.ImportElements();
        ArrayList AllElements = htElements.GetArrayList("elements");
        Hashtable firstElement = (Hashtable)AllElements[0];
        string eDescription = firstElement.GetString("description");

      Debug.Log("first verb: " + vDescription + ", first element: " + eDescription);
    }

    // Update is called once per frame
    void Update()
    {

    }
}