using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHistories.Services
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ImmanenceAttribute : System.Attribute
    {
        public ImmanenceAttribute(Type fallbackType)
        {
            FallbackType = fallbackType;
        }

    public Type FallbackType { get; private set; }
    }
}
