using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.UI;
using UnityEngine;

namespace Assets.Scripts.Editor
{
    [ExecuteAlways]
    public class AttentionGrabber : MonoBehaviour
    {
        [SerializeField]
        private bool lookAtMe = false;

        public void Update()
        {
            if(lookAtMe)
            {
                Debug.Log("Looking at " + gameObject.name);
                lookAtMe = false;
                Vector3 target = gameObject.transform.position;
                StartCoroutine(Watchman.Get<CameraZoom>().FocusOn(target, 0.5f));
            }
        }
    }
}
