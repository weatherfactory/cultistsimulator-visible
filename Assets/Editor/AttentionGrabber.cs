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
        [SerializeField]
        private float cameraMoveDuration = 2f;

        public void Update()
        {
            if(lookAtMe)
            {
                Debug.Log("Looking at " + gameObject.name);
                lookAtMe = false;
                Vector2 target = gameObject.transform.position;
                Debug.Log($"Focusing on {target}");
                Watchman.Get<CamOperator>().PointCameraAtTableLevelVector2(target, cameraMoveDuration);
            }
        }
    }
}
