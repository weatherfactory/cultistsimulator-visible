using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Enums;

namespace Assets.Scripts.Application.Spheres
{
    public class NavigationArgs
    {
        public int Index { get; set; }

        public NavigationAnimationDirection FirstNavigationDirection { get; set; }
        public NavigationAnimationDirection FinalNavigationDirection { get; set; }
        public NavigationAnimation.AnimationResponse OnBegin { get; set; }
        public NavigationAnimation.AnimationResponse OnEnd { get; set; }
        public NavigationAnimation.AnimationResponse OnInComplete { get; set; }
        public NavigationAnimation.AnimationResponse OnOutComplete { get; set; }
        public bool Instant { get; set; }


        public NavigationArgs(int index, NavigationAnimationDirection firstNavigationDirection, NavigationAnimationDirection finalNavigationDirection)
        {
            Index = index;
            FirstNavigationDirection = firstNavigationDirection;
            FinalNavigationDirection = finalNavigationDirection;
        }
    }
}
