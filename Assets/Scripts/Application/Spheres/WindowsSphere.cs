﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Constants;

namespace SecretHistories.TokenContainers
{
    public class WindowsSphere: Sphere
    {
        public override SphereCategory SphereCategory
        {
            get { return SphereCategory.World; }
        }
    }
}
