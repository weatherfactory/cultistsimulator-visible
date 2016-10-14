using UnityEngine;
using System.Collections;

public class Heartbeat : BoardMonoBehaviour {

	void Start () {
InvokeRepeating("UpdateQueue",0,1);	

	}

    void UpdateQueue()
    {
      //  BM.Log("Updated");
    }
}
