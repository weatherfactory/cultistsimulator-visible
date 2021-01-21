using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SecretHistories.UI

{
    public abstract class AbstractThresholdArrangement : MonoBehaviour
    {
        public virtual void ArrangeThreshold(ThresholdSphere threshold, int index)
        {
            threshold.viz.rectTrans.SetParent(this.transform);
            threshold.viz.rectTrans.localScale = Vector3.one;
            threshold.viz.rectTrans.localPosition = Vector3.zero;
            threshold.viz.rectTrans.localRotation = Quaternion.identity;
        }
    }

    //we can't use an interface if we want it to be a serializable field
    public class SimpleThresholdArrangement: AbstractThresholdArrangement
    {

    }
}
