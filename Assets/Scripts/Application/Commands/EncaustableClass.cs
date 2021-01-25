using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHistories.Commands
{
    /// <summary>
    /// Objects marked as Encaustable can be encausted to commands, for saving or reproduction.
    /// They should implement IEncaustable, and all public properties should be marked Encaust or DontEncaust.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EncaustableClass: System.Attribute
    {
        public Type ToType { get; private set; }

        public EncaustableClass(Type toType)
        {
            ToType = toType;
        }

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class Encaust : System.Attribute
    {
        
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DontEncaust : System.Attribute
    {

    }
}
