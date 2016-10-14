using UnityEngine;
using System.Collections;

public class Heartbeat : BoardMonoBehaviour {

	void Start () {
InvokeRepeating("Do",0,1);	

	}

    void Do()
    {
        BM.DoHeartbeat();
    }
}
