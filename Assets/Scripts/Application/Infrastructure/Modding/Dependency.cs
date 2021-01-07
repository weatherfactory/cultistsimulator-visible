using System;

namespace SecretHistories.Constants.Modding
{
    public class Dependency
    {
        public string ModId;
        public DependencyOperator VersionOperator;
        public Version Version;
    }
}
