using System.Reflection;

namespace Assets.Core.Fucine
{
    public class CachedFucineProperty
    {

        public PropertyInfo PropertyInfo { get; set; }
        public Fucine FucineAttribute { get; set; }
        public string Name => PropertyInfo.Name;
    }
}