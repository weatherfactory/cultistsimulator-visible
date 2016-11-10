using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.TabletopUi.Scripts
{
    public static class UnityEngineExtensions
    {
        public static T InstantiateLocally<T>(this GameObject g, Component original, Transform parent) where T: Component
        {
           T c=Object.Instantiate(original,parent,false) as T;
            c.transform.localScale=Vector3.one;
            return c;
        }
    }
}
