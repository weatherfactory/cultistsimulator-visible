using System;

namespace SecretHistories.Infrastructure.Modding
{
    public class Dependency
    {
        public string ModId;
        public DependencyOperator VersionOperator;
        public Version Version;
    }
}
