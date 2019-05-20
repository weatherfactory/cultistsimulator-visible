#if MODS
using System;

namespace Assets.TabletopUi.Scripts.Infrastructure.Modding
{
    public class Dependency
    {
        public string ModId;
        public DependencyOperator VersionOperator;
        public Version Version;
    }
}
#endif