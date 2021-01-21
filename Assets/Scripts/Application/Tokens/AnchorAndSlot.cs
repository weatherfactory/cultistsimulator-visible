using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.UI;
using SecretHistories.Constants;
using SecretHistories.Interfaces;
using SecretHistories.Spheres;

namespace SecretHistories.UI
{
    public class AnchorAndSlot
    {
        public Token Token { get; set; } 
        public Sphere Threshold { get; set; }
    }
}
