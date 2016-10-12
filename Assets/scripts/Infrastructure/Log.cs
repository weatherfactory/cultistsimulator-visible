using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace Assets.scripts.Infrastructure
{
    public static class SystemLog
    {
        public static void Write(string message)
        {
            //so I can use alternative methods later
            UnityEngine.Debug.Log(message);
        }
    }
}
