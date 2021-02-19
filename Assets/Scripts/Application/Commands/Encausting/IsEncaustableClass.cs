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
    public class IsEncaustableClass: System.Attribute
    {
        public Type ToType { get; private set; }

        public IsEncaustableClass(Type toType)
        {
            ToType = toType;
        }

    }

    //use this if we want to copy the encaustment information from a specified class - ie the opposite way from how we normally work
    //we expect the emulated class to be a base one, but nothing currently enforces this
    [AttributeUsage(AttributeTargets.Class)]
    public class IsEmulousEncaustable : System.Attribute
    {
        public Type EmulateBaseEncaustableType { get; private set; }

        public IsEmulousEncaustable(Type emulateBaseEncaustableType)
        {
            EmulateBaseEncaustableType = emulateBaseEncaustableType;
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
