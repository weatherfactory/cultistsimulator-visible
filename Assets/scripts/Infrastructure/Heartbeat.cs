using UnityEngine;
using System.Collections;

public class Heartbeat : BoardMonoBehaviour,ICharacterInfoSubscriber {

	void Start () {
InvokeRepeating("Do",0,1);	

	}

    void Do()
    {
        BM.DoHeartbeat();
    }

    public void ReceiveUpdate(Character character)
    {
        throw new System.NotImplementedException();
    }
}
